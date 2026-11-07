using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace AndroidDoctor.UI
{
    /// <summary>
    /// The one path every fix goes through: look it up, show "before -> after", ask for confirmation,
    /// apply, and report. A fix never runs without the user saying yes.
    /// </summary>
    public static class FixFlow
    {
        /// <summary>Runs the first action of a finding, asking with a modal dialog. True if a fix was applied.</summary>
        public static bool Run(Finding finding, out string message)
        {
            return Run(finding, finding.Actions.Count > 0 ? finding.Actions[0] : null, Ask(), out message);
        }

        /// <summary>Runs one action of a finding, asking with a modal dialog.</summary>
        public static bool Run(Finding finding, FixAction action, out string message)
        {
            return Run(finding, action, Ask(), out message);
        }

        /// <summary>
        /// "Fix all": one confirmation that lists every change, then every action that needs no decision.
        /// Actions that appear on several findings run once.
        /// </summary>
        public static bool RunAll(IList<Finding> findings, out string message)
        {
            return RunAll(findings, Ask(), out message);
        }

        /// <summary>How many different changes "Fix all" would make for these findings.</summary>
        public static int CountAllInOne(IList<Finding> findings) => CollectAllInOne(findings).Count;

        static Func<string, bool> Ask()
        {
            return description => EditorUtility.DisplayDialog(
                Messages.Get("fix.dialog.title"), description,
                Messages.Get("fix.dialog.apply"), Messages.Get("fix.dialog.cancel"));
        }

        static string Describe(IFix fix, Finding finding, FixAction action)
        {
            var actionFix = fix as IActionFix;
            return actionFix != null && action != null ? actionFix.Describe(finding, action) : fix.Describe(finding);
        }

        static bool Apply(IFix fix, Finding finding, FixAction action, out string message)
        {
            var actionFix = fix as IActionFix;
            return actionFix != null && action != null
                ? actionFix.Apply(finding, action, out message)
                : fix.Apply(finding, out message);
        }

        /// <summary>The first action of the finding, with a confirmation supplied by the caller.</summary>
        internal static bool Run(Finding finding, Func<string, bool> confirm, out string message)
        {
            return Run(finding, finding.Actions.Count > 0 ? finding.Actions[0] : null, confirm, out message);
        }

        /// <param name="confirm">Shows the description and returns true if the user accepts.</param>
        internal static bool Run(Finding finding, FixAction action, Func<string, bool> confirm, out string message)
        {
            message = null;
            if (action == null) return false;
            var fix = FixRegistry.Get(action.FixId);
            if (fix == null) return false;
            if (!confirm(Describe(fix, finding, action))) return false;
            bool applied = Apply(fix, finding, action, out message);
            if (applied && FixIds.ChangesSettings(action.FixId)) Gradle.LastBuildFailure.MarkAddressed();
            return applied;
        }

        internal static bool RunAll(IList<Finding> findings, Func<string, bool> confirm, out string message)
        {
            message = null;
            var items = CollectAllInOne(findings);
            if (items.Count == 0) return false;

            var text = new StringBuilder(Messages.Get("fix.all.header", items.Count));
            foreach (var item in items)
                text.Append("\n\n").Append(Describe(item.Fix, item.Finding, item.Action));
            if (!confirm(text.ToString())) return false;

            var results = new List<string>();
            int applied = 0;
            foreach (var item in items)
            {
                string one;
                bool ok;
                try { ok = Apply(item.Fix, item.Finding, item.Action, out one); }
                catch (Exception e) { ok = false; one = e.Message; }
                if (ok) applied++;
                if (!string.IsNullOrEmpty(one)) results.Add((ok ? "" : Messages.Get("fix.all.failed") + " ") + one);
            }
            if (applied > 0) Gradle.LastBuildFailure.MarkAddressed();
            message = Messages.Get("fix.all.done", applied, items.Count) + (results.Count > 0 ? "\n" + string.Join("\n", results) : "");
            return applied > 0;
        }

        sealed class Item { public Finding Finding; public FixAction Action; public IFix Fix; }

        static List<Item> CollectAllInOne(IList<Finding> findings)
        {
            var items = new List<Item>();
            var seen = new HashSet<string>();
            foreach (var finding in findings ?? new List<Finding>())
            {
                foreach (var action in finding.Actions.Where(a => a.AllInOne))
                {
                    if (!seen.Add(action.FixId + "|" + action.Arg)) continue;
                    var fix = FixRegistry.Get(action.FixId);
                    if (fix != null) items.Add(new Item { Finding = finding, Action = action, Fix = fix });
                }
            }
            return items;
        }
    }
}
