using System.IO;
using NUnit.Framework;
using AndroidDoctor.Native;

namespace AndroidDoctor.Tests
{
    public class ElfReaderTests
    {
        static ElfInfo Read(byte[] bytes)
        {
            ElfInfo info; string err;
            Assert.IsTrue(ElfReader.TryRead(new MemoryStream(bytes), out info, out err), err);
            return info;
        }

        static ElfBuilder.Seg Load(ulong vaddr, ulong memSize, ulong align)
        {
            return new ElfBuilder.Seg { Type = ElfBuilder.PT_LOAD, VAddr = vaddr, MemSize = memSize, Align = align };
        }

        static ElfBuilder.Seg Relro(ulong vaddr, ulong memSize)
        {
            return new ElfBuilder.Seg { Type = ElfBuilder.PT_GNU_RELRO, VAddr = vaddr, MemSize = memSize, Align = 1 };
        }

        static string Fixture(string name)
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(ElfBuilder).Assembly);
            return Path.Combine(package.resolvedPath, "Tests", "Editor", "Fixtures", "Native", name);
        }

        // --- the five mandatory tests from the design document ---

        [Test] public void Align16K_IsAccepted()
        {
            var info = Read(ElfBuilder.Elf64(0xB7, Load(0, 0x4000, 0x4000)));
            Assert.IsTrue(info.Is64);
            Assert.IsTrue(info.Is16KbLoadAligned);
        }

        [Test] public void Align4K_IsRejected()
        {
            var info = Read(ElfBuilder.Elf64(0xB7, Load(0, 0x1000, 0x1000)));
            Assert.IsFalse(info.Is16KbLoadAligned);
        }

        [Test] public void OneBadLoadAmongGoodOnes_IsRejected()
        {
            var info = Read(ElfBuilder.Elf64(0xB7, Load(0, 0x4000, 0x4000), Load(0, 0x1000, 0x1000)));
            Assert.AreEqual(0x1000UL, info.MinLoadAlign());
            Assert.IsFalse(info.Is16KbLoadAligned);
        }

        [Test] public void MisalignedRelro_IsDetected()
        {
            var info = Read(ElfBuilder.Elf64(0xB7, Load(0, 0x4000, 0x4000), Relro(0x1000, 0x800)));
            Assert.IsFalse(info.IsRelroAligned16Kb);
        }

        [Test] public void NotElf_FailsWithoutThrowing()
        {
            ElfInfo info; string err;
            Assert.IsFalse(ElfReader.TryRead(new MemoryStream(new byte[] { 1, 2, 3 }), out info, out err));
            Assert.IsNull(info);
            Assert.IsNotEmpty(err);
        }

        // --- ELF32 ---

        [Test] public void Elf32_IsReadWithItsOwnLayout()
        {
            var info = Read(ElfBuilder.Elf32(0x28, Load(0, 0x1000, 0x1000), Load(0x2400, 0xC00, 0x1000)));
            Assert.IsFalse(info.Is64);
            Assert.AreEqual(0x28, info.Machine);
            Assert.AreEqual(2, info.Loads.Count);
            Assert.AreEqual(0x1000UL, info.MinLoadAlign());
            Assert.AreEqual(0x2400UL, info.Loads[1].VAddr);
        }

        // --- RELRO: Google accepts three shapes ---

        [Test] public void Relro_Absent_IsAligned()
        {
            Assert.IsTrue(Read(ElfBuilder.Elf64(0xB7, Load(0, 0x4000, 0x4000))).IsRelroAligned16Kb);
        }

        [Test] public void Relro_EndingOnA16KbBoundary_IsAligned()
        {
            var info = Read(ElfBuilder.Elf64(0xB7, Load(0, 0x8000, 0x4000), Relro(0x2000, 0x2000)));
            Assert.IsTrue(info.IsRelroAligned16Kb); // 0x2000 + 0x2000 = 0x4000
        }

        [Test] public void Relro_FillingTheRestOfItsLoadSegment_IsAligned()
        {
            // The shape of a real library built with only max-page-size=16384: RELRO ends at 0x9000, which is not
            // a multiple of 0x4000, but it is also where its LOAD segment ends.
            var info = Read(ElfBuilder.Elf64(0xB7,
                Load(0, 0x578, 0x4000), Load(0x8630, 0x9D0, 0x4000), Relro(0x8630, 0x9D0)));
            Assert.IsTrue(info.IsRelroAligned16Kb);
        }

        [Test] public void Relro_StoppingShortOfItsLoadEnd_IsMisaligned()
        {
            var info = Read(ElfBuilder.Elf64(0xB7,
                Load(0, 0x578, 0x4000), Load(0x8630, 0x9D0, 0x4000), Relro(0x8630, 0x100)));
            Assert.IsFalse(info.IsRelroAligned16Kb);
        }

        // --- robustness ---

        [Test] public void TruncatedFile_FailsWithoutThrowing()
        {
            byte[] full = ElfBuilder.Elf64(0xB7, Load(0, 0x4000, 0x4000));
            var cut = new byte[full.Length - 20];
            System.Array.Copy(full, cut, cut.Length);
            ElfInfo info; string err;
            Assert.IsFalse(ElfReader.TryRead(new MemoryStream(cut), out info, out err));
            Assert.IsNotEmpty(err);
        }

        [Test] public void BigEndian_IsRefused()
        {
            byte[] bytes = ElfBuilder.Elf64(0xB7, Load(0, 0x4000, 0x4000));
            bytes[5] = 2; // EI_DATA = big endian
            ElfInfo info; string err;
            Assert.IsFalse(ElfReader.TryRead(new MemoryStream(bytes), out info, out err));
        }

        [Test] public void AbsurdProgramHeaderCount_IsRefused()
        {
            byte[] bytes = ElfBuilder.Elf64(0xB7, Load(0, 0x4000, 0x4000));
            bytes[0x38] = 0xFF; bytes[0x39] = 0xFF; // e_phnum = 65535
            ElfInfo info; string err;
            Assert.IsFalse(ElfReader.TryRead(new MemoryStream(bytes), out info, out err));
        }

        [Test] public void MissingFile_FailsWithoutThrowing()
        {
            ElfInfo info; string err;
            Assert.IsFalse(ElfReader.TryReadFile("Z:/does/not/exist.so", out info, out err));
            Assert.IsNotEmpty(err);
        }

        [TestCase((ushort)0xB7, "arm64-v8a")]
        [TestCase((ushort)0x3E, "x86_64")]
        [TestCase((ushort)0x28, "armeabi-v7a")]
        [TestCase((ushort)0x03, "x86")]
        [TestCase((ushort)0x08, null)]
        public void AbiFromMachine(ushort machine, string expected)
        {
            Assert.AreEqual(expected, ElfReader.AbiFromMachine(machine));
        }

        // --- real libraries built with the NDK that ships with Unity ---

        [Test] public void RealLibrary_Built4K_IsNotAligned()
        {
            ElfInfo info; string err;
            Assert.IsTrue(ElfReader.TryReadFile(Fixture("elf64-load4k.bin"), out info, out err), err);
            Assert.IsTrue(info.Is64);
            Assert.AreEqual(0x1000UL, info.MinLoadAlign());
            Assert.IsFalse(info.Is16KbLoadAligned);
        }

        [Test] public void RealLibrary_Built16K_IsAligned_AndItsRelroIsAccepted()
        {
            ElfInfo info; string err;
            Assert.IsTrue(ElfReader.TryReadFile(Fixture("elf64-load16k.bin"), out info, out err), err);
            Assert.AreEqual(0x4000UL, info.MinLoadAlign());
            Assert.IsTrue(info.Is16KbLoadAligned);
            Assert.IsTrue(info.IsRelroAligned16Kb, "RELRO fills its LOAD segment, which Google's guide accepts");
        }

        [Test] public void RealLibrary_32Bit_IsRecognised()
        {
            ElfInfo info; string err;
            Assert.IsTrue(ElfReader.TryReadFile(Fixture("elf32-load4k.bin"), out info, out err), err);
            Assert.IsFalse(info.Is64);
            Assert.AreEqual("armeabi-v7a", ElfReader.AbiFromMachine(info.Machine));
        }
    }
}
