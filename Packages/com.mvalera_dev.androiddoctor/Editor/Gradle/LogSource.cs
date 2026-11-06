using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AndroidDoctor.Gradle
{
    /// <summary>Where the log of the last build can be read from.</summary>
    public static class LogSource
    {
        static readonly Regex SavedAt = new Regex(@"Build log was saved at '([^']+)'");

        /// <summary>
        /// The Editor log this session writes to. Unity reports it through Application.consoleLogPath, and it is
        /// not always the platform default (an Editor started with -logFile, for example, writes elsewhere).
        /// </summary>
        public static string EditorLogPath()
        {
            string reported = Application.consoleLogPath;
            if (!string.IsNullOrEmpty(reported) && File.Exists(reported)) return reported;
            return PlatformDefaultEditorLogPath();
        }

        // The locations from the Unity manual, used when Unity does not report a log path.
        internal static string PlatformDefaultEditorLogPath()
        {
#if UNITY_EDITOR_WIN
            return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Unity", "Editor", "Editor.log");
#elif UNITY_EDITOR_OSX
            return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Library", "Logs", "Unity", "Editor.log");
#else
            return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), ".config", "unity3d", "Editor.log");
#endif
        }

        /// <summary>Reads only the end of a file (2 MB by default). Unity keeps the log open, so share it.</summary>
        public static string ReadTail(string path, int maxBytes = 2000000)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long start = Math.Max(0, fs.Length - maxBytes);
                fs.Seek(start, SeekOrigin.Begin);
                using (var sr = new StreamReader(fs)) return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// The Gradle log that Unity says it saved for the most recent Gradle run ("Build log was saved at '...'"),
        /// or null if the text does not mention one.
        /// </summary>
        public static string FindGradleLogPath(string editorLogText)
        {
            if (string.IsNullOrEmpty(editorLogText)) return null;
            string path = null;
            foreach (Match m in SavedAt.Matches(editorLogText)) path = m.Groups[1].Value;
            return path;
        }
    }
}
