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
            if (fromEditorLog != null) return SucceededAfter(editorLogText, fromEditorLog.Block) ? null : fromEditorLog;

            // The failure may be cut off in a big log; try the Gradle log Unity says it saved.
            string gradleLogPath = LogSource.FindGradleLogPath(editorLogText);
            string gradleLog = gradleLogPath == null ? null : readFile(gradleLogPath);
            var fromFile = GradleLogParser.Extract(gradleLog);
            return fromFile == null || SucceededAfter(editorLogText, "Build log was saved at") ? null : fromFile;
        }

        // Unity prints this line when a build finishes well. If it comes after the failure, the failure is old news.
        static readonly string[] SuccessMarkers = { "Build completed with a result of 'Succeeded'", "Build Finished, Result: Success" };

        /// <summary>True when a successful build is reported after the last place where <paramref name="failureMarker"/> appears.</summary>
        internal static bool SucceededAfter(string log, string failureMarker)
        {
            if (string.IsNullOrEmpty(log) || string.IsNullOrEmpty(failureMarker)) return false;
            string first = failureMarker.Length > 60 ? failureMarker.Substring(0, 60) : failureMarker;
            int failureAt = log.LastIndexOf(first, StringComparison.Ordinal);
            if (failureAt < 0) return false;
            foreach (string marker in SuccessMarkers)
                if (log.LastIndexOf(marker, StringComparison.Ordinal) > failureAt) return true;
            return false;
        }

        static string ReadFileOrNull(string path)
        {
            try { return File.Exists(path) ? File.ReadAllText(path) : null; }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }
    }
}
