using System.IO;
using System.Text.RegularExpressions;

namespace AndroidDoctor.Project
{
    /// <summary>Splits a library file name like "play-services-basement-18.3.0.aar" into artifact and version.</summary>
    public static class ArtifactName
    {
        // artifact, then "-", then a version that starts with a digit and runs to the extension.
        static readonly Regex Versioned = new Regex(
            @"^(?<name>.+?)-(?<version>\d+(?:\.\d+)*(?:[-.][A-Za-z0-9]+)*)$");

        /// <summary>
        /// Parses a file name (with or without folders). Version is null when the name carries none
        /// ("foo.aar", "foo-release.aar"). Returns false for anything that is not an .aar or .jar.
        /// </summary>
        public static bool TryParse(string fileName, out string artifact, out string version)
        {
            artifact = null; version = null;
            if (string.IsNullOrEmpty(fileName)) return false;
            string name = Path.GetFileName(fileName);
            string ext = Path.GetExtension(name).ToLowerInvariant();
            if (ext != ".aar" && ext != ".jar") return false;

            string stem = name.Substring(0, name.Length - ext.Length);
            if (stem.Length == 0) return false;
            var m = Versioned.Match(stem);
            if (m.Success)
            {
                artifact = m.Groups["name"].Value;
                version = m.Groups["version"].Value;
            }
            else
            {
                artifact = stem;
            }
            return true;
        }
    }
}
