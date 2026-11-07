using UnityEditor;

namespace AndroidDoctor.Gradle
{
    /// <summary>
    /// The failure of the last Android build as the checks should show it. A failure stays in the Editor log until the
    /// next build, so once the user has applied a fix it is marked as addressed and no longer shown. A build that fails
    /// again logs a new failure, which clears the mark; a build that works clears the failure itself.
    /// </summary>
    public static class LastBuildFailure
    {
        const string AddressedKey = "AndroidDoctor.AddressedFailure";

        static GradleError Latest() => GradleErrorCapture.Get() ?? LastBuildAnalyzer.Analyze();

        static string Key(GradleError error) => (error.Block ?? "").GetHashCode().ToString();

        /// <summary>The failure to show, or null when there is none or the user has already acted on it.</summary>
        public static GradleError Current()
        {
            var error = Latest();
            if (error == null) return null;
            return SessionState.GetString(AddressedKey, "") == Key(error) ? null : error;
        }

        /// <summary>Called after a fix was applied: whatever failed before is no longer the news.</summary>
        public static void MarkAddressed()
        {
            var error = Latest();
            if (error != null) SessionState.SetString(AddressedKey, Key(error));
        }

        /// <summary>Called when a build fails or succeeds: the next failure, if any, must be shown.</summary>
        public static void ForgetAddressed() => SessionState.EraseString(AddressedKey);
    }
}
