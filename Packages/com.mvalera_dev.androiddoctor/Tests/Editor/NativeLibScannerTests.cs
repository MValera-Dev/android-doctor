using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Native;

namespace AndroidDoctor.Tests
{
    public class NativeLibScannerTests
    {
        static byte[] Elf4K(ushort machine = 0xB7)
        {
            return ElfBuilder.Elf64(machine, new ElfBuilder.Seg { Type = ElfBuilder.PT_LOAD, MemSize = 0x1000, Align = 0x1000 });
        }

        static byte[] Elf16K(ushort machine = 0xB7)
        {
            return ElfBuilder.Elf64(machine, new ElfBuilder.Seg { Type = ElfBuilder.PT_LOAD, MemSize = 0x4000, Align = 0x4000 });
        }

        static void WriteBytes(TempDirectory tmp, string relative, byte[] bytes)
        {
            string full = Path.Combine(tmp.Root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllBytes(full, bytes);
        }

        static void WriteAar(TempDirectory tmp, string relative, params (string entry, byte[] bytes)[] entries)
        {
            string full = Path.Combine(tmp.Root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            using (var fs = new FileStream(full, FileMode.Create))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                foreach (var (entry, bytes) in entries)
                    using (var s = zip.CreateEntry(entry).Open()) s.Write(bytes, 0, bytes.Length);
            }
        }

        static ScanRoot AssetsRoot(TempDirectory tmp) =>
            new ScanRoot { Path = tmp.Dir("Assets"), AssetPrefix = "Assets" };

        static System.Collections.Generic.List<NativeLib> Scan(ScanRoot root, Func<string, bool> filter = null) =>
            NativeLibScanner.Scan(new[] { root }, filter ?? (p => true));

        // --- loose .so ---

        [Test] public void LooseSo_ReadsAbiFromFolder_ElfData_AndOwner()
        {
            using (var tmp = new TempDirectory())
            {
                WriteBytes(tmp, "Assets/Plugins/Android/Vendor/arm64-v8a/libfoo.so", Elf4K());

                var lib = Scan(AssetsRoot(tmp)).Single();

                Assert.AreEqual("libfoo.so", lib.Name);
                Assert.AreEqual("arm64-v8a", lib.Abi);
                Assert.IsTrue(lib.Is64Bit);
                Assert.AreEqual("Assets/Plugins/Android/Vendor/arm64-v8a/libfoo.so", lib.Location);
                Assert.AreEqual("Assets/Plugins/Android/Vendor", lib.Owner);
                Assert.IsNotNull(lib.Elf);
                Assert.IsNull(lib.ReadError);
                Assert.IsFalse(lib.Elf.Is16KbLoadAligned);
            }
        }

        [Test] public void LooseSo_WithoutAbiFolder_GetsAbiFromElfMachine()
        {
            using (var tmp = new TempDirectory())
            {
                WriteBytes(tmp, "Assets/Native/libbaz.so", Elf16K(0x3E));
                var lib = Scan(AssetsRoot(tmp)).Single();
                Assert.AreEqual("x86_64", lib.Abi);
                Assert.AreEqual("Assets/Native", lib.Owner);
            }
        }

        [Test] public void LooseSo_UnreadableFile_KeepsTheReasonAndNeverThrows()
        {
            using (var tmp = new TempDirectory())
            {
                WriteBytes(tmp, "Assets/Plugins/Android/arm64-v8a/libjunk.so", new byte[] { 1, 2, 3, 4 });
                var lib = Scan(AssetsRoot(tmp)).Single();
                Assert.IsNull(lib.Elf);
                Assert.IsNotEmpty(lib.ReadError);
                Assert.AreEqual("arm64-v8a", lib.Abi); // still known from the folder
            }
        }

        [Test] public void LooseSo_InAPackageRoot_IsOwnedByThePackage()
        {
            using (var tmp = new TempDirectory())
            {
                WriteBytes(tmp, "pkg/Runtime/Plugins/Android/arm64-v8a/libsdk.so", Elf16K());
                var root = new ScanRoot { Path = tmp.Dir("pkg"), AssetPrefix = "Packages/com.vendor.sdk", IsPackage = true };

                var lib = Scan(root).Single();

                Assert.AreEqual("Packages/com.vendor.sdk", lib.Owner);
                Assert.AreEqual("Packages/com.vendor.sdk/Runtime/Plugins/Android/arm64-v8a/libsdk.so", lib.Location);
            }
        }

        // --- the Android filter (the desktop Linux plugin false positive) ---

        [Test] public void FilesThatAreNotAndroidPlugins_AreIgnored()
        {
            using (var tmp = new TempDirectory())
            {
                WriteBytes(tmp, "Assets/Plugins/x86_64/libdesktop.so", Elf4K(0x3E));
                WriteBytes(tmp, "Assets/Plugins/Android/arm64-v8a/libandroid.so", Elf4K());

                var libs = Scan(AssetsRoot(tmp), assetPath => !assetPath.Contains("Plugins/x86_64/"));

                Assert.AreEqual("libandroid.so", libs.Single().Name);
            }
        }

        [Test] public void FoldersUnityIgnores_AreNotScanned()
        {
            using (var tmp = new TempDirectory())
            {
                WriteBytes(tmp, "Assets/Samples~/arm64-v8a/liba.so", Elf4K());
                WriteBytes(tmp, "Assets/.hidden/arm64-v8a/libb.so", Elf4K());
                WriteBytes(tmp, "Assets/Library/arm64-v8a/libc.so", Elf4K());
                Assert.IsEmpty(Scan(AssetsRoot(tmp)));
            }
        }

        // --- .aar ---

        [Test] public void Aar_ListsItsJniLibraries_WithAbiAndOwner()
        {
            using (var tmp = new TempDirectory())
            {
                WriteAar(tmp, "Assets/Plugins/Android/foo-1.0.0.aar",
                    ("AndroidManifest.xml", new byte[] { 1 }),
                    ("classes.jar", new byte[] { 2 }),
                    ("jni/arm64-v8a/libq.so", Elf4K()),
                    ("jni/x86/libq.so", Elf4K(0x03)),
                    ("jni/readme.txt", new byte[] { 3 }));

                var libs = Scan(AssetsRoot(tmp)).OrderBy(l => l.Abi).ToList();

                Assert.AreEqual(2, libs.Count);
                Assert.AreEqual("arm64-v8a", libs[0].Abi);
                Assert.AreEqual("Assets/Plugins/Android/foo-1.0.0.aar", libs[0].Owner);
                Assert.AreEqual("Assets/Plugins/Android/foo-1.0.0.aar!/jni/arm64-v8a/libq.so", libs[0].Location);
                Assert.IsFalse(libs[0].Elf.Is16KbLoadAligned);
                Assert.AreEqual("x86", libs[1].Abi);
            }
        }

        [Test] public void Aar_ThatIsNotAZip_BecomesOneUnreadableEntry()
        {
            using (var tmp = new TempDirectory())
            {
                WriteBytes(tmp, "Assets/Plugins/Android/broken.aar", new byte[] { 1, 2, 3, 4, 5 });
                var lib = Scan(AssetsRoot(tmp)).Single();
                Assert.AreEqual("broken.aar", lib.Name);
                Assert.IsNotEmpty(lib.ReadError);
                Assert.IsNull(lib.Abi);
            }
        }

        [Test] public void Aar_FilteredOutAsNotAndroid_IsIgnored()
        {
            using (var tmp = new TempDirectory())
            {
                WriteAar(tmp, "Assets/Plugins/Android/foo-1.0.0.aar", ("jni/arm64-v8a/libq.so", Elf4K()));
                Assert.IsEmpty(Scan(AssetsRoot(tmp), p => false));
            }
        }

        // --- helpers ---

        [TestCase("Assets/Plugins/Android/arm64-v8a/libfoo.so", "arm64-v8a")]
        [TestCase("Assets/Plugins/x86_64/libfoo.so", "x86_64")]
        [TestCase("Assets/Plugins/armeabi-v7a/libfoo.so", "armeabi-v7a")]
        [TestCase("Assets/Plugins/libfoo.so", null)]
        public void AbiFromPath(string path, string expected)
        {
            Assert.AreEqual(expected, NativeLibScanner.AbiFromPath(path));
        }

        [TestCase("Assets/Plugins/Android/Vendor/arm64-v8a/libfoo.so", "Assets/Plugins/Android/Vendor")]
        [TestCase("Assets/Plugins/Vendor/Android/libs/arm64-v8a/libfoo.so", "Assets/Plugins/Vendor")]
        [TestCase("Assets/Plugins/Android/arm64-v8a/libfoo.so", "Assets/Plugins/Android")]
        [TestCase("Assets/Plugins/x86_64/libfoo.so", "Assets/Plugins")]
        [TestCase("Assets/Foo/libfoo.so", "Assets/Foo")]
        [TestCase("Assets/libfoo.so", "Assets")]
        public void OwnerInAssets(string path, string expected)
        {
            Assert.AreEqual(expected, NativeLibScanner.OwnerInAssets(path));
        }
    }
}
