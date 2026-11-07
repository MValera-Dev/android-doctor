using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AndroidDoctor.Project
{
    /// <summary>
    /// Properties that the Android Gradle plugin used in 7.x and removed in 8.3. Unity 6 ships a newer plugin, so a
    /// gradleTemplate.properties carried over from Unity 2022 makes Gradle fail with "Failed to apply plugin".
    /// Pure text work, so it is easy to test.
    /// </summary>
    public static class GradleProperties
    {
        const string Replacement = "android.useFullClasspathForDexingTransform=true";

        static readonly Regex Removed = new Regex(
            @"^\s*android\.enableDexingArtifactTransform(?:\.desugaring)?\s*=.*$", RegexOptions.IgnoreCase);
        static readonly Regex Replaced = new Regex(
            @"^\s*android\.useFullClasspathForDexingTransform\s*=", RegexOptions.IgnoreCase);

        /// <summary>The removed property lines of a properties file, as written.</summary>
        public static List<string> FindRemoved(string text)
        {
            return (text ?? "").Replace("\r\n", "\n").Split('\n').Where(l => Removed.IsMatch(l)).Select(l => l.Trim()).ToList();
        }

        /// <summary>
        /// Drops the removed properties and, once, adds the property that replaces them (unless the file has it).
        /// Every other line, comments included, stays as it was.
        /// </summary>
        public static string Migrate(string text, out int changed)
        {
            changed = 0;
            if (string.IsNullOrEmpty(text)) return text;
            bool crlf = text.Contains("\r\n");
            var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
            bool hasReplacement = lines.Any(l => Replaced.IsMatch(l));

            var output = new List<string>();
            foreach (string line in lines)
            {
                if (!Removed.IsMatch(line)) { output.Add(line); continue; }
                changed++;
                if (!hasReplacement) { output.Add(Replacement); hasReplacement = true; }
            }
            return string.Join(crlf ? "\r\n" : "\n", output);
        }
    }
}
