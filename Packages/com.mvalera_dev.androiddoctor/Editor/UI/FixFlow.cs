using System;
using UnityEditor;

namespace AndroidDoctor.UI
{
    /// <summary>
    /// The one path every fix goes through: look it up, show "before -> after", ask for confirmation,
    /// apply, and report. A fix never runs without the user saying yes.
    /// </summary>
    public static class FixFlow
    {
        /// <summary>Runs the fix behind a finding, asking with a modal dialog. True if a fix was applied.</summary>
        public static bool Run(Finding finding, out string message)
        {
            return Run(finding, description => EditorUtility.DisplayDialog(
                Messages.Get("fix.dialog.title"), description,
                Messages.Get("fix.dialog.apply"), Messages.Get("fix.dialog.cancel")), out message);
        }

        /// <param name="confirm">Shows the description and returns true if the user accepts.</param>
        internal static bool Run(Finding finding, Func<string, bool> confirm, out string message)
        {
            message = null;
            var fix = FixRegistry.Get(finding.FixId);
            if (fix == null) return false;
            if (!confirm(fix.Describe(finding))) return false;
            return fix.Apply(finding, out message);
        }
    }
}
