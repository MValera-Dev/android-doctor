using System;
using System.IO;

namespace AndroidDoctor.Gradle
{
    /// <summary>
    /// Finds the failure of the last Android build. Unity 6000.6 prints the failure into the Editor log and also saves
    /// the full Gradle output as a separate file, so both are tried.
    /// </summary>
    public static class LastBuildAnalyzer
    {
        /// <summary>Reads the real Editor log. Null when no Gradle failure is found or the log cannot be read.</summary>
        public static GradleError Analyze()
        {
            try { return Analyze(LogSource.ReadTail(LogSource.EditorLogPath()), ReadFileOrNull); }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }

        /// <param name="editorLogText">The end of the Editor log.</param>
        /// <param name="readFile">Reads a file by path; returns null if it cannot be read.</param>
        internal static GradleError Analyze(string editorLogText, Func<string, string> readFile)
        {
            var fromEditorLog = GradleLogParser.Extract(editorLogText);
            if (fromEditorLog != null) return fromEditorLog;

            // The failure may be cut off in a big log; try the Gradle log Unity says it saved.
            string gradleLogPath = LogSource.FindGradleLogPath(editorLogText);
            string gradleLog = gradleLogPath == null ? null : readFile(gradleLogPath);
            return GradleLogParser.Extract(gradleLog);
        }

        static string ReadFileOrNull(string path)
        {
            try { return File.Exists(path) ? File.ReadAllText(path) : null; }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }
    }
}
