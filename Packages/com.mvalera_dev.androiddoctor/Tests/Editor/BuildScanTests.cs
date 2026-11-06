using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Build;
using AndroidDoctor.Native;

namespace AndroidDoctor.Tests
{
    public class BuildScanTests
    {
        static byte[] Elf(ulong align, ushort machine = 0xB7)
        {
            return ElfBuilder.Elf64(machine, new ElfBuilder.Seg { Type = ElfBuilder.PT_LOAD, MemSize = 0x4000, Align = align });
        }

        static string WriteZip(TempDirectory tmp, string name, params (string entry, byte[] bytes)[] entries)
        {
            string full = Path.Combine(tmp.Root, name);
            using (var fs = new FileStream(full, FileMode.Create))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                foreach (var (entry, bytes) in entries)
                    using (var s = zip.CreateEntry(entry).Open()) s.Write(bytes, 0, bytes.Length);
            }
            return full;
        }

        static NativeLib ProjectLib(string owner, string name, string abi)
        {
            return new NativeLib { Owner = owner, Name = name, Abi = abi, Location = owner + "/" + name };
        }

        static NativeLib BuildLib(string name, string abi, ulong align)
        {
            var elf = new ElfInfo { Is64 = abi == "arm64-v8a" || abi == "x86_64" };
            elf.Loads.Add(new ElfSegment { Align = align, MemSize = 0x4000 });
            return new NativeLib { Name = name, Abi = abi, Elf = elf, Owner = ArtifactScanner.BuildOutputOwner, Location = "app.apk!/lib/" + abi + "/" + name };
        }

        // --- ArtifactScanner ---

        [Test] public void Apk_ListsLibEntries_AndIgnoresEverythingElse()
        {
            using (var tmp = new TempDirectory())
            {
                string apk = WriteZip(tmp, "app.apk",
                    ("AndroidManifest.xml", new byte[] { 1 }),
                    ("classes.dex", new byte[] { 2 }),
                    ("lib/arm64-v8a/libmain.so", Elf(0x4000)),
                    ("lib/arm64-v8a/libbad.so", Elf(0x1000)),
                    ("lib/armeabi-v7a/libmain.so", Elf(0x1000, 0x28)),
                    ("lib/arm64-v8a/notes.txt", new byte[] { 3 }),
                    ("assets/lib/x.so", new byte[] { 4 }));

                List<NativeLib> libs; string error;
                Assert.IsTrue(ArtifactScanner.TryScan(apk, out libs, out error), error);

                Assert.AreEqual(3, libs.Count);
                var bad = libs.Single(l => l.Name == "libbad.so");
                Assert.AreEqual("arm64-v8a", bad.Abi);
                Assert.AreEqual(ArtifactScanner.BuildOutputOwner, bad.Owner);
                StringAssert.Contains("!/lib/arm64-v8a/libbad.so", bad.Location);
                Assert.IsFalse(bad.Elf.Is16KbLoadAligned);
                Assert.IsTrue(libs.Single(l => l.Name == "libmain.so" && l.Abi == "arm64-v8a").Elf.Is16KbLoadAligned);
            }
        }

        [Test] public void Aab_ReadsBaseAndFeatureModules()
        {
            using (var tmp = new TempDirectory())
            {
                string aab = WriteZip(tmp, "app.aab",
                    ("base/lib/arm64-v8a/libbase.so", Elf(0x1000)),
                    ("feature1/lib/x86_64/libfeat.so", Elf(0x4000, 0x3E)),
                    ("base/dex/classes.dex", new byte[] { 1 }));

                List<NativeLib> libs; string error;
                Assert.IsTrue(ArtifactScanner.TryScan(aab, out libs, out error), error);

                Assert.AreEqual(2, libs.Count);
                Assert.IsTrue(libs.Any(l => l.Name == "libbase.so" && l.Abi == "arm64-v8a"));
                Assert.IsTrue(libs.Any(l => l.Name == "libfeat.so" && l.Abi == "x86_64"));
            }
        }

        [Test] public void Scan_OfAFileThatIsNotAZip_FailsWithAReasonAndNeverThrows()
        {
            using (var tmp = new TempDirectory())
            {
                string path = tmp.File("fake.apk", "not a zip");
                List<NativeLib> libs; string error;
                Assert.IsFalse(ArtifactScanner.TryScan(path, out libs, out error));
                Assert.IsNotEmpty(error);
                Assert.IsEmpty(libs);
            }
        }

        [Test] public void Scan_OfAMissingFile_Fails()
        {
            List<NativeLib> libs; string error;
            Assert.IsFalse(ArtifactScanner.TryScan("Z:/does/not/exist.apk", out libs, out error));
            Assert.IsNotEmpty(error);
        }

        [Test] public void Scan_KeepsALibraryWithABrokenElfAsUnreadable()
        {
            using (var tmp = new TempDirectory())
            {
                string apk = WriteZip(tmp, "app.apk", ("lib/arm64-v8a/libjunk.so", new byte[] { 1, 2, 3 }));
                List<NativeLib> libs; string error;
                Assert.IsTrue(ArtifactScanner.TryScan(apk, out libs, out error));
                Assert.IsNull(libs.Single().Elf);
                Assert.IsNotEmpty(libs.Single().ReadError);
            }
        }

        // --- BLD001 attribution ---

        [Test] public void Bld001_LinksABadBuildLibraryToItsProjectOwner_ByNameAndAbi()
        {
            var build = new List<NativeLib> { BuildLib("libfoo.so", "arm64-v8a", 0x1000) };
            var project = new List<NativeLib>
            {
                ProjectLib("Packages/com.vendor.sdk", "libfoo.so", "arm64-v8a"),
                ProjectLib("Assets/Plugins/Other", "libfoo.so", "armeabi-v7a"), // same name, other ABI: not a match
            };

            var f = BuildAlignmentRule.Evaluate(TestContexts.Create(), build, project).Single();

            Assert.AreEqual("BLD001", f.RuleId);
            Assert.AreEqual(Category.Build, f.Category);
            Assert.AreEqual(Severity.Error, f.Severity);
            StringAssert.Contains("Packages/com.vendor.sdk", f.Title);
            StringAssert.DoesNotContain("Assets/Plugins/Other", f.Title);
            StringAssert.Contains("libfoo.so (arm64-v8a)", f.Evidence[0]);
        }

        [Test] public void Bld001_ALibraryNotInTheProject_SaysSoAndDoesNotInventAnOwner()
        {
            var build = new List<NativeLib> { BuildLib("libtransitive.so", "arm64-v8a", 0x1000) };

            var f = BuildAlignmentRule.Evaluate(TestContexts.Create(), build, new List<NativeLib>()).Single();

            StringAssert.Contains(Messages.Get("BLD001.owner.unknown"), f.Title);
            StringAssert.Contains(Messages.Get("BLD001.detail.unknown"), f.Detail);
        }

        [Test] public void Bld001_GroupsByOwner_AndListsKnownOwnersBeforeUnknown()
        {
            var build = new List<NativeLib>
            {
                BuildLib("libz.so", "arm64-v8a", 0x1000),      // unknown
                BuildLib("liba1.so", "arm64-v8a", 0x1000),
                BuildLib("liba2.so", "arm64-v8a", 0x1000),
            };
            var project = new List<NativeLib>
            {
                ProjectLib("Assets/Plugins/A", "liba1.so", "arm64-v8a"),
                ProjectLib("Assets/Plugins/A", "liba2.so", "arm64-v8a"),
            };

            var findings = BuildAlignmentRule.Evaluate(TestContexts.Create(), build, project).ToList();

            Assert.AreEqual(2, findings.Count);
            StringAssert.Contains("Assets/Plugins/A", findings[0].Title);
            Assert.AreEqual(2, findings[0].Evidence.Count);
            StringAssert.Contains(Messages.Get("BLD001.owner.unknown"), findings[1].Title);
        }

        [Test] public void Bld001_AlignedAnd32BitLibraries_AreNotReported()
        {
            var build = new List<NativeLib>
            {
                BuildLib("libok.so", "arm64-v8a", 0x4000),
                BuildLib("libold.so", "armeabi-v7a", 0x1000),
            };
            Assert.IsEmpty(BuildAlignmentRule.Evaluate(TestContexts.Create(), build, new List<NativeLib>()));
        }

        // --- BLD001 end to end through Run ---

        [Test] public void Bld001_Run_WithoutAnArtifact_IsSkipped()
        {
            Assert.IsEmpty(new BuildAlignmentRule().Run(TestContexts.Create()));
        }

        [Test] public void Bld001_Run_ReadsTheArtifactAndReportsTheBadLibrary()
        {
            using (var tmp = new TempDirectory())
            {
                var ctx = TestContexts.Create();
                ctx.ArtifactPath = WriteZip(tmp, "app.apk", ("lib/arm64-v8a/libbad.so", Elf(0x1000)), ("lib/arm64-v8a/libgood.so", Elf(0x4000)));

                var f = new BuildAlignmentRule().Run(ctx).Single();

                Assert.AreEqual(Severity.Error, f.Severity);
                Assert.AreEqual(1, f.Evidence.Count);
                StringAssert.Contains("libbad.so", f.Evidence[0]);
            }
        }

        [Test] public void Bld001_Run_ACleanBuild_ProducesNothing()
        {
            using (var tmp = new TempDirectory())
            {
                var ctx = TestContexts.Create();
                ctx.ArtifactPath = WriteZip(tmp, "app.apk", ("lib/arm64-v8a/libgood.so", Elf(0x4000)));
                Assert.IsEmpty(new BuildAlignmentRule().Run(ctx));
            }
        }

        [Test] public void Bld001_Run_AFileThatCannotBeRead_IsOneInfoFinding()
        {
            using (var tmp = new TempDirectory())
            {
                var ctx = TestContexts.Create();
                ctx.ArtifactPath = tmp.File("fake.apk", "not a zip");
                var f = new BuildAlignmentRule().Run(ctx).Single();
                Assert.AreEqual(Severity.Info, f.Severity);
            }
        }

        // --- remembered artifact ---

        [Test] public void ArtifactState_RemembersAnExistingFile_AndForgetsAMissingOne()
        {
            string original = UnityEditor.SessionState.GetString(BuildArtifactState.Key, "");
            try
            {
                using (var tmp = new TempDirectory())
                {
                    string path = tmp.File("app.apk", "x");
                    BuildArtifactState.Set(path);
                    Assert.AreEqual(path, BuildArtifactState.Get());

                    File.Delete(path);
                    Assert.IsNull(BuildArtifactState.Get());

                    BuildArtifactState.Clear();
                    Assert.IsNull(BuildArtifactState.Get());
                }
            }
            finally { UnityEditor.SessionState.SetString(BuildArtifactState.Key, original); }
        }

        [Test] public void ContextCreate_PicksUpTheRememberedArtifact()
        {
            string original = UnityEditor.SessionState.GetString(BuildArtifactState.Key, "");
            try
            {
                using (var tmp = new TempDirectory())
                {
                    string path = tmp.File("app.apk", "x");
                    BuildArtifactState.Set(path);
                    Assert.AreEqual(path, RuleContext.Create().ArtifactPath);
                }
            }
            finally { UnityEditor.SessionState.SetString(BuildArtifactState.Key, original); }
        }
    }
}
