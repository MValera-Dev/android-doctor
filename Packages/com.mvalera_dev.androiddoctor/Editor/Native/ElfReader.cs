using System.Collections.Generic;
using System.IO;

namespace AndroidDoctor.Native
{
    public struct ElfSegment { public ulong Offset, VAddr, FileSize, MemSize, Align; }

    public sealed class ElfInfo
    {
        public const ulong Page16Kb = 0x4000;

        public bool Is64;
        public ushort Machine;
        public readonly List<ElfSegment> Loads = new List<ElfSegment>();
        public ElfSegment? Relro;

        public ulong MinLoadAlign()
        {
            if (Loads.Count == 0) return 0;
            ulong min = ulong.MaxValue;
            foreach (var l in Loads) if (l.Align < min) min = l.Align;
            return min;
        }

        /// <summary>What Google Play validates: no LOAD segment may have p_align below 0x4000.</summary>
        public bool Is16KbLoadAligned => Loads.Count > 0 && MinLoadAlign() >= Page16Kb;

        /// <summary>
        /// Google's guide accepts a library when it has no GNU_RELRO, when (VirtAddr + MemSiz) is a multiple of
        /// 0x4000, or when GNU_RELRO fills the rest of the LOAD segment that contains it.
        /// </summary>
        public bool IsRelroAligned16Kb
        {
            get
            {
                if (!Relro.HasValue) return true;
                var relro = Relro.Value;
                ulong relroEnd = relro.VAddr + relro.MemSize;
                if (relroEnd % Page16Kb == 0) return true;
                foreach (var load in Loads)
                {
                    bool contains = relro.VAddr >= load.VAddr && relro.VAddr < load.VAddr + load.MemSize;
                    if (contains) return relroEnd >= load.VAddr + load.MemSize;
                }
                return false;
            }
        }
    }

    public static class ElfReader
    {
        const uint PT_LOAD = 1, PT_GNU_RELRO = 0x6474E552;
        const int MaxProgramHeaders = 256; // real libraries have a dozen; more means junk, not an ELF

        public const ushort EM_386 = 0x03, EM_ARM = 0x28, EM_X86_64 = 0x3E, EM_AARCH64 = 0xB7;

        /// <summary>Reads only the header and the program header table, with seeks, never the whole file.</summary>
        public static bool TryRead(Stream s, out ElfInfo info, out string error)
        {
            info = null; error = null;
            try
            {
                var r = new BinaryReader(s); // BinaryReader reads little-endian
                s.Seek(0, SeekOrigin.Begin);
                byte[] id = r.ReadBytes(16);
                if (id.Length < 16 || id[0] != 0x7F || id[1] != (byte)'E' || id[2] != (byte)'L' || id[3] != (byte)'F')
                { error = "not an ELF file"; return false; }
                if (id[5] != 1) { error = "big-endian ELF is not supported"; return false; }
                bool is64 = id[4] == 2;
                if (id[4] != 1 && !is64) { error = "unknown ELF class"; return false; }

                s.Seek(0x12, SeekOrigin.Begin);
                ushort machine = r.ReadUInt16();

                ulong phoff; ushort phentsize, phnum;
                if (is64)
                {
                    s.Seek(0x20, SeekOrigin.Begin); phoff = r.ReadUInt64();
                    s.Seek(0x36, SeekOrigin.Begin); phentsize = r.ReadUInt16(); phnum = r.ReadUInt16();
                }
                else
                {
                    s.Seek(0x1C, SeekOrigin.Begin); phoff = r.ReadUInt32();
                    s.Seek(0x2A, SeekOrigin.Begin); phentsize = r.ReadUInt16(); phnum = r.ReadUInt16();
                }
                if (phnum > MaxProgramHeaders) { error = "implausible program header count"; return false; }

                var result = new ElfInfo { Is64 = is64, Machine = machine };
                for (int i = 0; i < phnum; i++)
                {
                    s.Seek((long)phoff + (long)i * phentsize, SeekOrigin.Begin);
                    uint type = r.ReadUInt32();
                    var seg = new ElfSegment();
                    if (is64)
                    {
                        r.ReadUInt32();                              // p_flags
                        seg.Offset = r.ReadUInt64(); seg.VAddr = r.ReadUInt64();
                        r.ReadUInt64();                              // p_paddr
                        seg.FileSize = r.ReadUInt64(); seg.MemSize = r.ReadUInt64(); seg.Align = r.ReadUInt64();
                    }
                    else
                    {
                        seg.Offset = r.ReadUInt32(); seg.VAddr = r.ReadUInt32();
                        r.ReadUInt32();                              // p_paddr
                        seg.FileSize = r.ReadUInt32(); seg.MemSize = r.ReadUInt32();
                        r.ReadUInt32();                              // p_flags
                        seg.Align = r.ReadUInt32();
                    }
                    if (type == PT_LOAD) result.Loads.Add(seg);
                    else if (type == PT_GNU_RELRO) result.Relro = seg;
                }
                info = result;
                return true;
            }
            catch (EndOfStreamException) { error = "truncated ELF file"; return false; }
            catch (IOException e) { error = e.Message; return false; }
        }

        /// <summary>Opens a file for reading (sharing allowed) and reads its ELF header.</summary>
        public static bool TryReadFile(string path, out ElfInfo info, out string error)
        {
            info = null;
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    return TryRead(fs, out info, out error);
            }
            catch (IOException e) { error = e.Message; return false; }
            catch (System.UnauthorizedAccessException e) { error = e.Message; return false; }
        }

        /// <summary>The Android ABI folder name for an ELF machine type, or null if it is not an Android ABI.</summary>
        public static string AbiFromMachine(ushort machine)
        {
            switch (machine)
            {
                case EM_AARCH64: return "arm64-v8a";
                case EM_X86_64: return "x86_64";
                case EM_ARM: return "armeabi-v7a";
                case EM_386: return "x86";
                default: return null;
            }
        }
    }
}
