using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AndroidDoctor.Environment
{
    /// <summary>Reads the revision from source.properties at the root of an NDK folder.</summary>
    public static class NdkProperties
    {
        /// <summary>The Pkg.Revision value ("27.2.12479018"), or null if unreadable.</summary>
        public static string ReadRevision(string ndkFolder)
        {
            if (string.IsNullOrEmpty(ndkFolder)) return null;
            try
            {
                string file = Path.Combine(ndkFolder, "source.properties");
                if (!File.Exists(file)) return null;
                foreach (string line in File.ReadAllLines(file))
                {
                    var m = Regex.Match(line, @"^\s*Pkg\.Revision\s*=\s*(\S+)\s*$");
                    if (m.Success) return m.Groups[1].Value;
                }
                return null;
            }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }
    }
}
