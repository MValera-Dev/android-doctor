using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AndroidDoctor.Environment
{
    /// <summary>Reads the Java version from the "release" file at the root of a JDK folder.</summary>
    public static class JdkRelease
    {
        /// <summary>The JAVA_VERSION value ("17.0.18", "1.8.0_292", "21"), or null if unreadable.</summary>
        public static string ReadVersion(string jdkFolder)
        {
            if (string.IsNullOrEmpty(jdkFolder)) return null;
            try
            {
                string file = Path.Combine(jdkFolder, "release");
                if (!File.Exists(file)) return null;
                foreach (string line in File.ReadAllLines(file))
                {
                    var m = Regex.Match(line, "^\\s*JAVA_VERSION\\s*=\\s*\"?([^\"\\s]+)\"?\\s*$");
                    if (m.Success) return m.Groups[1].Value;
                }
                return null;
            }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }

        /// <summary>Major version of a Java version string: "17.0.18" is 17, "1.8.0_292" is 8. -1 if unparseable.</summary>
        public static int ParseMajor(string version)
        {
            if (string.IsNullOrEmpty(version)) return -1;
            var m = Regex.Match(version, @"^(\d+)(?:\.(\d+))?");
            if (!m.Success) return -1;
            int first = int.Parse(m.Groups[1].Value);
            if (first == 1 && m.Groups[2].Success) return int.Parse(m.Groups[2].Value);
            return first;
        }
    }
}
