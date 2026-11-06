using System.IO;
using UnityEditor.PackageManager;

namespace AndroidDoctor
{
    /// <summary>Locates and reads the JSON data files shipped in Editor/Data/.</summary>
    internal static class DataFiles
    {
        /// <summary>Returns the file contents, or null if the package or the file cannot be found.</summary>
        public static string ReadText(string fileName)
        {
            try
            {
                var package = PackageInfo.FindForAssembly(typeof(DataFiles).Assembly);
                if (package == null || string.IsNullOrEmpty(package.resolvedPath)) return null;
                string path = Path.Combine(package.resolvedPath, "Editor", "Data", fileName);
                return File.Exists(path) ? File.ReadAllText(path) : null;
            }
            catch (IOException) { return null; }
        }
    }
}
