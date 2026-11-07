using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace AndroidDoctor.UI
{
    /// <summary>The Checks tab content: a summary line and the findings grouped by category.</summary>
    public sealed class ChecksPanel : VisualElement
    {
        readonly Label _summary;
        readonly ScrollView _results;
        readonly Action<Finding, FixAction> _onFix;
        readonly Action<string> _notify;

        public string SummaryText => _summary.text;
        public ScrollView Results => _results;

        public ChecksPanel(Action<Finding, FixAction> onFix, Action<string> notify)
        {
            _onFix = onFix;
            _notify = notify;
            style.flexGrow = 1;

            _summary = new Label();
            _summary.style.marginTop = 4;
            _summary.style.marginBottom = 4;
            Add(_summary);

            _results = new ScrollView();
            _results.style.flexGrow = 1;
            _results.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            Add(_results);

            ShowIdle();
        }

        /// <summary>Nothing has run yet. The window never runs checks by itself.</summary>
        public void ShowIdle()
        {
            _results.Clear();
            _summary.text = Messages.Get("ui.idle");
        }

        public void Show(IList<Finding> findings)
        {
            _results.Clear();
            _summary.text = Summarize(findings);

            // One foldout per category, in the order of the enum, only for categories that have findings.
            foreach (var group in findings.GroupBy(f => f.Category).OrderBy(g => g.Key))
            {
                var foldout = new Foldout
                {
                    text = Messages.Get("ui.category." + group.Key) + " (" + group.Count() + ")",
                    value = true,
                };
                foreach (var finding in group) foldout.Add(new FindingCard(finding, _onFix, _notify));
                _results.Add(foldout);
            }
        }

        /// <summary>One message for a whole run that failed, with a button to copy the exception.</summary>
        public void ShowInternalError(Exception e)
        {
            _results.Clear();
            _summary.text = Messages.Get("ui.error.title");
            _results.Add(new Label(e.GetType().Name + ": " + e.Message));
            _results.Add(new Button(() => UnityEditor.EditorGUIUtility.systemCopyBuffer = e.ToString())
            { text = Messages.Get("ui.error.copy") });
        }

        internal static string Summarize(IList<Finding> findings)
        {
            if (findings.Count == 0) return Messages.Get("ui.noFindings");
            int errors = findings.Count(f => f.Severity == Severity.Error);
            int warnings = findings.Count(f => f.Severity == Severity.Warning);
            int infos = findings.Count(f => f.Severity == Severity.Info);

            var parts = new List<string>();
            if (errors > 0) parts.Add(Messages.Get(errors == 1 ? "ui.errors.one" : "ui.errors.many", errors));
            if (warnings > 0) parts.Add(Messages.Get(warnings == 1 ? "ui.warnings.one" : "ui.warnings.many", warnings));
            if (infos > 0) parts.Add(Messages.Get("ui.infos", infos));
            return string.Join(", ", parts);
        }
    }
}
