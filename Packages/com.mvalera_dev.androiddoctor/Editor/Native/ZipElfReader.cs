using System.IO;
using System.IO.Compression;

namespace AndroidDoctor.Native
{
    /// <summary>Reads the ELF header of a library stored inside a zip (.aar, .apk, .aab).</summary>
    public static class ZipElfReader
    {
        const long FirstChunk = 65536;

        // The program header table normally sits right after the ELF header, so the first 64 KB almost always
        // contain it. If they do not, read the whole entry. A zip entry stream cannot seek, hence the copy.
        public static ElfInfo Read(ZipArchiveEntry entry, out string error)
        {
            ElfInfo info;
            foreach (long limit in new long[] { FirstChunk, long.MaxValue })
            {
                using (var s = entry.Open())
                using (var ms = new MemoryStream())
                {
                    CopyUpTo(s, ms, limit);
                    if (ElfReader.TryRead(ms, out info, out error)) return info;
                    if (limit == long.MaxValue) return null;
                }
            }
            error = "unreachable";
            return null;
        }

        static void CopyUpTo(Stream src, Stream dst, long max)
        {
            var buf = new byte[81920]; long total = 0; int n;
            while (total < max && (n = src.Read(buf, 0, (int)System.Math.Min(buf.Length, max - total))) > 0)
            { dst.Write(buf, 0, n); total += n; }
            dst.Position = 0;
        }
    }
}
