using System;
using System.Collections.Generic;
using System.IO;

namespace AndroidDoctor.Project
{
    /// <summary>
    /// Recursive file search that follows Unity's own rules: folders starting with "." or ending with "~"
    /// are ignored by the importer, and Library, Temp, obj, Logs and node_modules are never project content.
    /// Access errors are ignored.
    /// </summary>
    public static class ProjectFileWalker
    {
        static readonly HashSet<string> SkippedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Library", "Temp", "obj", "Logs", "node_modules",
        };

        /// <summary>Full paths of every file under root for which the filter returns true.</summary>
        public static List<string> FindFiles(string root, Func<string, bool> filter)
        {
            var found = new List<string>();
            if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) Walk(root, filter, found);
            return found;
        }

        /// <summary>
        /// "C:\Proj\Assets\a.aar" becomes "Assets/a.aar" when it sits under the project folder; any other
        /// path is returned with forward slashes.
        /// </summary>
        public static string ToProjectRelative(string projectPath, string fullPath)
        {
            string path = (fullPath ?? "").Replace('\\', '/');
            if (string.IsNullOrEmpty(projectPath)) return path;
            string root = projectPath.Replace('\\', '/').TrimEnd('/') + "/";
            return path.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? path.Substring(root.Length) : path;
        }

        static bool IsSkipped(string folderName)
        {
            return folderName.StartsWith(".") || folderName.EndsWith("~") || SkippedFolders.Contains(folderName);
        }

        static void Walk(string folder, Func<string, bool> filter, List<string> found)
        {
            try
            {
                foreach (string file in Directory.GetFiles(folder))
                    if (filter(file)) found.Add(file);
                foreach (string sub in Directory.GetDirectories(folder))
                    if (!IsSkipped(Path.GetFileName(sub))) Walk(sub, filter, found);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
