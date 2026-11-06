using System.IO;
using UnityEditor;

namespace AndroidDoctor.Build
{
    /// <summary>
    /// Remembers, for this Editor session, which APK or AAB the build checks should look at: the last one
    /// built, or the one the user picked with Scan build. Main thread only (SessionState).
    /// </summary>
    public static class BuildArtifactState
    {
        public const string Key = "AndroidDoctor.LastArtifact";

        public static void Set(string path) => SessionState.SetString(Key, path ?? "");

        /// <summary>The remembered path if that file still exists, otherwise null.</summary>
        public static string Get()
        {
            string path = SessionState.GetString(Key, "");
            return !string.IsNullOrEmpty(path) && File.Exists(path) ? path : null;
        }

        public static void Clear() => SessionState.EraseString(Key);
    }
}
