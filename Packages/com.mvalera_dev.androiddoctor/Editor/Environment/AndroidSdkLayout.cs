using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AndroidDoctor.Environment
{
    /// <summary>Read-only questions about the folder layout of an Android SDK.</summary>
    public static class AndroidSdkLayout
    {
        // "android-34", "android-37.0": the API level is the number right after "android-".
        static readonly Regex PlatformFolder = new Regex(@"^android-(\d+)", RegexOptions.IgnoreCase);

        /// <summary>Installed platform API levels, sorted ascending and without duplicates.</summary>
        public static List<int> InstalledApis(string sdkPath)
        {
            var apis = new SortedSet<int>();
            foreach (string name in SubfolderNames(sdkPath, "platforms"))
            {
                var m = PlatformFolder.Match(name);
                if (m.Success) apis.Add(int.Parse(m.Groups[1].Value));
            }
            return apis.ToList();
        }

        public static bool HasPlatform(string sdkPath, int api) => InstalledApis(sdkPath).Contains(api);

        /// <summary>Highest installed platform API level, or -1 if there are none.</summary>
        public static int HighestApi(string sdkPath)
        {
            var apis = InstalledApis(sdkPath);
            return apis.Count == 0 ? -1 : apis[apis.Count - 1];
        }

        public static bool HasPlatformTools(string sdkPath) => FolderExists(sdkPath, "platform-tools");

        /// <summary>True if cmdline-tools has at least one version folder ("16.0", "latest"), or the legacy tools folder has content.</summary>
        public static bool HasCommandLineTools(string sdkPath)
        {
            if (SubfolderNames(sdkPath, "cmdline-tools").Any()) return true;
            return !string.IsNullOrEmpty(sdkPath) && DirectoryHasEntries(Path.Combine(sdkPath, "tools"));
        }

        public static List<string> InstalledBuildTools(string sdkPath)
        {
            return SubfolderNames(sdkPath, "build-tools").OrderBy(n => n, StringComparer.Ordinal).ToList();
        }

        static bool FolderExists(string sdkPath, string sub)
        {
            return !string.IsNullOrEmpty(sdkPath) && Directory.Exists(Path.Combine(sdkPath, sub));
        }

        static bool DirectoryHasEntries(string path)
        {
            try { return Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any(); }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        static IEnumerable<string> SubfolderNames(string sdkPath, string sub)
        {
            if (string.IsNullOrEmpty(sdkPath)) return Enumerable.Empty<string>();
            try
            {
                string dir = Path.Combine(sdkPath, sub);
                if (!Directory.Exists(dir)) return Enumerable.Empty<string>();
                return Directory.GetDirectories(dir).Select(Path.GetFileName).ToList();
            }
            catch (IOException) { return Enumerable.Empty<string>(); }
            catch (UnauthorizedAccessException) { return Enumerable.Empty<string>(); }
        }
    }
}
