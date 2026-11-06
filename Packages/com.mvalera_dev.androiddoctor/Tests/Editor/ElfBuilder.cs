using System.IO;

namespace AndroidDoctor.Tests
{
    /// <summary>Builds minimal synthetic ELF files in memory, so tests of ElfReader need no binaries.</summary>
    public static class ElfBuilder
    {
        public sealed class Seg { public uint Type; public ulong VAddr, MemSize, Align; }

        public const uint PT_LOAD = 1, PT_GNU_RELRO = 0x6474E552;

        public static byte[] Elf64(ushort machine, params Seg[] segs)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(new byte[] { 0x7F, (byte)'E', (byte)'L', (byte)'F', 2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }); // e_ident
                w.Write((ushort)3);            // e_type = ET_DYN (0x10)
                w.Write(machine);              // e_machine (0x12)
                w.Write(1u);                   // e_version (0x14)
                w.Write(0UL);                  // e_entry (0x18)
                w.Write(64UL);                 // e_phoff (0x20)
                w.Write(0UL);                  // e_shoff (0x28)
                w.Write(0u);                   // e_flags (0x30)
                w.Write((ushort)64);           // e_ehsize (0x34)
                w.Write((ushort)56);           // e_phentsize (0x36)
                w.Write((ushort)segs.Length);  // e_phnum (0x38)
                w.Write((ushort)0); w.Write((ushort)0); w.Write((ushort)0); // shentsize, shnum, shstrndx
                foreach (var s in segs)
                {
                    w.Write(s.Type); w.Write(5u);              // p_type, p_flags
                    w.Write(0UL); w.Write(s.VAddr); w.Write(s.VAddr); // p_offset, p_vaddr, p_paddr
                    w.Write(s.MemSize); w.Write(s.MemSize); w.Write(s.Align); // p_filesz, p_memsz, p_align
                }
                return ms.ToArray();
            }
        }

        public static byte[] Elf32(ushort machine, params Seg[] segs)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(new byte[] { 0x7F, (byte)'E', (byte)'L', (byte)'F', 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }); // e_ident
                w.Write((ushort)3);            // e_type
                w.Write(machine);              // e_machine (0x12)
                w.Write(1u);                   // e_version (0x14)
                w.Write(0u);                   // e_entry (0x18)
                w.Write(52u);                  // e_phoff (0x1C)
                w.Write(0u);                   // e_shoff (0x20)
                w.Write(0u);                   // e_flags (0x24)
                w.Write((ushort)52);           // e_ehsize (0x28)
                w.Write((ushort)32);           // e_phentsize (0x2A)
                w.Write((ushort)segs.Length);  // e_phnum (0x2C)
                w.Write((ushort)0); w.Write((ushort)0); w.Write((ushort)0); // shentsize, shnum, shstrndx
                foreach (var s in segs)
                {
                    w.Write(s.Type);                           // p_type
                    w.Write(0u); w.Write((uint)s.VAddr); w.Write((uint)s.VAddr); // p_offset, p_vaddr, p_paddr
                    w.Write((uint)s.MemSize); w.Write((uint)s.MemSize);          // p_filesz, p_memsz
                    w.Write(5u); w.Write((uint)s.Align);       // p_flags, p_align
                }
                return ms.ToArray();
            }
        }
    }
}
