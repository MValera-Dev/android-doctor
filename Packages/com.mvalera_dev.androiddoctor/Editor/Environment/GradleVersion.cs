using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AndroidDoctor.Environment
{
    /// <summary>Reads a Gradle distribution's version from lib/gradle-launcher-X.Y.Z.jar.</summary>
    public static class GradleVersion
    {
        static readonly Regex LauncherJar = new Regex(@"^gradle-launcher-(\d+(?:\.\d+)*(?:-[A-Za-z0-9.]+)?)\.jar$", RegexOptions.IgnoreCase);

        /// <summary>The version in the launcher jar's file name ("9.3.1"), or null if not found.</summary>
        public static string FromLauncherJar(string gradleFolder)
        {
            if (string.IsNullOrEmpty(gradleFolder)) return null;
            try
            {
                string lib = Path.Combine(gradleFolder, "lib");
                if (!Directory.Exists(lib)) return null;
                foreach (string file in Directory.GetFiles(lib, "gradle-launcher-*.jar"))
                {
                    var m = LauncherJar.Match(Path.GetFileName(file));
                    if (m.Success) return m.Groups[1].Value;
                }
                return null;
            }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }
    }
}
