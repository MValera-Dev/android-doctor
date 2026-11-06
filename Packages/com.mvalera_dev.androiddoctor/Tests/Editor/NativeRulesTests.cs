using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Native;

namespace AndroidDoctor.Tests
{
    /// <summary>NAT001, NAT002 and NAT003, fed with hand-built library lists.</summary>
    public class NativeRulesTests
    {
        static NativeLib Lib(string owner, string name, string abi, ulong loadAlign = 0x4000, bool relroAligned = true)
        {
            var elf = new ElfInfo { Is64 = abi == "arm64-v8a" || abi == "x86_64" };
            elf.Loads.Add(new ElfSegment { Align = loadAlign, VAddr = 0, MemSize = 0x4000 });
            if (!relroAligned)
                elf.Relro = new ElfSegment { VAddr = 0x1000, MemSize = 0x800 }; // ends at 0x1800, short of the LOAD end
            return new NativeLib
            {
                Name = name, Abi = abi, Owner = owner, Elf = elf,
                Location = owner + "/" + abi + "/" + name,
            };
        }

        static RuleContext Context() => TestContexts.Create();

        // --- NAT001 ---

        [Test] public void Nat001_MisalignedLibraries_AreGroupedByOwner()
        {
            var libs = new List<NativeLib>
            {
                Lib("Assets/Plugins/VendorA", "liba1.so", "arm64-v8a", 0x1000),
                Lib("Assets/Plugins/VendorA", "liba2.so", "x86_64", 0x1000),
                Lib("Assets/Plugins/VendorB", "libb.so", "arm64-v8a", 0x1000),
                Lib("Assets/Plugins/VendorC", "libc.so", "arm64-v8a", 0x4000),
            };

            var findings = NativeAlignmentRule.Evaluate(Context(), libs).Where(f => f.Severity == Severity.Error).ToList();

            Assert.AreEqual(2, findings.Count);
            Assert.AreEqual("NAT001", findings[0].RuleId);
            StringAssert.Contains("VendorA", findings[0].Title);
            Assert.AreEqual(2, findings[0].Evidence.Count);
            StringAssert.Contains("liba1.so (arm64-v8a)", findings[0].Evidence[0]);
            StringAssert.Contains("1000", findings[0].Evidence[0]);
            StringAssert.Contains("VendorB", findings[1].Title);
        }

        [Test] public void Nat001_AlignedLibraries_ProduceNothing()
        {
            var libs = new List<NativeLib> { Lib("Assets/Plugins/V", "liba.so", "arm64-v8a", 0x4000), Lib("Assets/Plugins/V", "libb.so", "x86_64", 0x10000) };
            Assert.IsEmpty(NativeAlignmentRule.Evaluate(Context(), libs));
        }

        [Test] public void Nat001_Misaligned32BitLibraries_AreNeverJudged()
        {
            var libs = new List<NativeLib> { Lib("Assets/Plugins/V", "liba.so", "armeabi-v7a", 0x1000), Lib("Assets/Plugins/V", "libb.so", "x86", 0x1000) };
            var findings = NativeAlignmentRule.Evaluate(Context(), libs).ToList();
            Assert.IsFalse(findings.Any(f => f.Severity == Severity.Error));
            var info = findings.Single(); // one Info line counting them
            Assert.AreEqual(Severity.Info, info.Severity);
            StringAssert.Contains("2", info.Title);
        }

        [Test] public void Nat001_PackageOwner_NamesTheInstalledVersion()
        {
            var ctx = Context();
            ctx.Packages.Add(new PackageRef { Name = "com.vendor.sdk", Version = "2.4.1", ResolvedPath = "x" });
            var libs = new List<NativeLib> { Lib("Packages/com.vendor.sdk", "libsdk.so", "arm64-v8a", 0x1000) };

            var f = NativeAlignmentRule.Evaluate(ctx, libs).Single();

            StringAssert.Contains("com.vendor.sdk", f.Detail);
            StringAssert.Contains("2.4.1", f.Detail);
        }

        [Test] public void Nat001_UnreadableLibraries_AreOneInfoFindingNeverAnError()
        {
            var bad = new NativeLib { Name = "libjunk.so", Abi = "arm64-v8a", Owner = "Assets/Plugins/V", Location = "Assets/Plugins/V/libjunk.so", ReadError = "not an ELF file" };
            var bad2 = new NativeLib { Name = "broken.aar", Owner = "Assets/Plugins/broken.aar", Location = "Assets/Plugins/broken.aar", ReadError = "zip" };

            var f = NativeAlignmentRule.Evaluate(Context(), new List<NativeLib> { bad, bad2 }).Single();

            Assert.AreEqual(Severity.Info, f.Severity);
            Assert.AreEqual(2, f.Evidence.Count);
            Assert.IsTrue(f.Evidence.Any(e => e.Contains("not an ELF file")));
            Assert.IsTrue(f.Evidence.Any(e => e.Contains("broken.aar")));
        }

        [Test] public void Nat001_NoLibraries_ProduceNothing()
        {
            Assert.IsEmpty(NativeAlignmentRule.Evaluate(Context(), new List<NativeLib>()));
        }

        // --- NAT002 ---

        [Test] public void Nat002_MisalignedRelro_IsWarningGroupedByOwner()
        {
            var libs = new List<NativeLib>
            {
                Lib("Assets/Plugins/V", "liba.so", "arm64-v8a", 0x4000, relroAligned: false),
                Lib("Assets/Plugins/V", "libb.so", "arm64-v8a", 0x4000, relroAligned: true),
            };

            var f = RelroAlignmentRule.Evaluate(Context(), libs).Single();

            Assert.AreEqual("NAT002", f.RuleId);
            Assert.AreEqual(Severity.Warning, f.Severity);
            Assert.AreEqual(1, f.Evidence.Count);
            StringAssert.Contains("liba.so", f.Evidence[0]);
            StringAssert.Contains("1800", f.Evidence[0]);
        }

        [Test] public void Nat002_32BitLibraries_AreNotJudged()
        {
            var libs = new List<NativeLib> { Lib("Assets/Plugins/V", "liba.so", "armeabi-v7a", 0x1000, relroAligned: false) };
            Assert.IsEmpty(RelroAlignmentRule.Evaluate(Context(), libs));
        }

        // --- NAT003 ---

        [Test] public void Nat003_OnlyThirtyTwoBit_IsError()
        {
            var libs = new List<NativeLib>
            {
                Lib("Assets/Plugins/V", "libold.so", "armeabi-v7a"),
                Lib("Assets/Plugins/V", "libold.so", "x86"),
            };

            var f = Missing64BitRule.Evaluate(Context(), libs).Single();

            Assert.AreEqual("NAT003", f.RuleId);
            Assert.AreEqual(Severity.Error, f.Severity);
            StringAssert.Contains("libold.so", f.Evidence[0]);
            StringAssert.Contains("armeabi-v7a", f.Evidence[0]);
        }

        [Test] public void Nat003_ThirtyTwoBitPlusSixtyFour_IsFine()
        {
            var libs = new List<NativeLib>
            {
                Lib("Assets/Plugins/V", "libok.so", "armeabi-v7a"),
                Lib("Assets/Plugins/V", "libok.so", "arm64-v8a"),
            };
            Assert.IsEmpty(Missing64BitRule.Evaluate(Context(), libs));
        }

        [Test] public void Nat003_SixtyFourBitOnly_IsFine()
        {
            Assert.IsEmpty(Missing64BitRule.Evaluate(Context(), new List<NativeLib> { Lib("Assets/Plugins/V", "libnew.so", "arm64-v8a") }));
        }

        [Test] public void Nat003_SameNameInDifferentOwners_IsNotMixedUp()
        {
            var libs = new List<NativeLib>
            {
                Lib("Assets/Plugins/A", "libfoo.so", "armeabi-v7a"),
                Lib("Assets/Plugins/B", "libfoo.so", "arm64-v8a"),
            };
            var f = Missing64BitRule.Evaluate(Context(), libs).Single();
            StringAssert.Contains("Assets/Plugins/A", f.Title);
        }

        [Test] public void Nat003_UnknownAbi_IsIgnored()
        {
            var lib = new NativeLib { Name = "broken.aar", Owner = "Assets/Plugins/broken.aar", ReadError = "zip" };
            Assert.IsEmpty(Missing64BitRule.Evaluate(Context(), new List<NativeLib> { lib }));
        }
    }
}
