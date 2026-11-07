using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AndroidDoctor.Gradle
{
    /// <summary>
    /// Listens to the console. When Unity reports a failed Gradle build it keeps the text for this session and logs a
    /// warning with the probable cause. The console text can be cut, so the Editor log stays the complete source.
    /// </summary>
    [InitializeOnLoad]
    public static class GradleErrorCapture
    {
        public const string SessionKey = "AndroidDoctor.LastGradleError";
        static bool _reporting;

        static GradleErrorCapture()
        {
            // The main-thread callback: SessionState can only be used there.
            Application.logMessageReceived -= OnLog;
            Application.logMessageReceived += OnLog;
        }

        internal static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (_reporting) return; // our own warning below comes back through this callback
            if (type != LogType.Error && type != LogType.Exception) return;

            var error = GradleLogParser.Extract(condition);
            if (error == null) return;

            _reporting = true;
            try
            {
                SessionState.SetString(SessionKey, condition);
                LastBuildFailure.ForgetAddressed();
                var main = GradleTranslator.Translate(error).First();
                Debug.LogWarning(main.RuleId == GradleTranslator.UnknownId
                    ? Messages.Get("gradle.capture.unknown")
                    : Messages.Get("gradle.capture.warning", main.Title, main.RuleId));
            }
            finally { _reporting = false; }
        }

        /// <summary>The failure captured earlier in this session, or null.</summary>
        public static GradleError Get() => GradleLogParser.Extract(SessionState.GetString(SessionKey, ""));

        public static void Clear() => SessionState.EraseString(SessionKey);
    }
}
