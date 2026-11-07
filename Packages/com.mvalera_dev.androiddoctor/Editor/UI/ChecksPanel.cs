using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndroidDoctor.UI
{
    /// <summary>
    /// The Checks tab: one sentence about the whole project, the buttons that act on it as a whole, and the findings
    /// grouped by how much they matter. Errors stop the build, warnings are worth fixing, notes are only information.
    /// </summary>
    public sealed class ChecksPanel : VisualElement
    {
        readonly Image _verdictIcon;
        readonly Label _verdictTitle;
        readonly Label _verdictSub;
        readonly Button _runButton;
        readonly Button _fixAllButton;
        readonly Label _buildFile;
        readonly ScrollView _results;
        readonly Action<Finding, FixAction> _onFix;
        readonly Action<string> _notify;

        public string SummaryText => _verdictTitle.text;
        public string DetailText => _verdictSub.text;
        public ScrollView Results => _results;
        public Button RunButton => _runButton;
        public Button FixAllButton => _fixAllButton;

        public ChecksPanel(Action<Finding, FixAction> onFix, Action<string> notify,
            Action onRun = null, Action onFixAll = null, Action onScanBuild = null, Action onCopyReport = null)
        {
            _onFix = onFix;
            _notify = notify;
            UiStyles.Apply(this);
            style.flexGrow = 1;

            // Verdict
            var verdict = new VisualElement();
            verdict.AddToClassList("ad-verdict");
            _verdictIcon = new Image { scaleMode = ScaleMode.ScaleToFit };
            _verdictIcon.AddToClassList("ad-verdict-icon");
            verdict.Add(_verdictIcon);

            var text = new VisualElement();
            text.AddToClassList("ad-verdict-text");
            _verdictTitle = new Label();
            _verdictTitle.AddToClassList("ad-verdict-title");
            _verdictSub = new Label();
            _verdictSub.AddToClassList("ad-verdict-sub");
            _verdictSub.AddToClassList("ad-muted");
            text.Add(_verdictTitle);
            text.Add(_verdictSub);
            verdict.Add(text);

            var buttons = new VisualElement();
            buttons.AddToClassList("ad-verdict-buttons");
            _fixAllButton = new Button(() => onFixAll?.Invoke());
            _runButton = new Button(() => onRun?.Invoke());
            buttons.Add(_fixAllButton);
            buttons.Add(_runButton);
            verdict.Add(buttons);
            Add(verdict);

            _results = new ScrollView();
            _results.AddToClassList("ad-results");
            _results.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            Add(_results);

            _buildFile = new Label();
            _buildFile.AddToClassList("ad-build-file");
            _buildFile.AddToClassList("ad-muted");
            _buildFile.style.display = DisplayStyle.None;
            Add(_buildFile);

            var footer = new VisualElement();
            footer.AddToClassList("ad-footer");
            var scan = new Button(() => onScanBuild?.Invoke()) { text = Messages.Get("ui.scanBuild") };
            var copy = new Button(() => onCopyReport?.Invoke()) { text = Messages.Get("ui.copyReport") };
            scan.AddToClassList("ad-link");
            copy.AddToClassList("ad-link");
            footer.Add(scan);
            footer.Add(copy);
            Add(footer);

            ShowIdle();
        }

        /// <summary>Nothing has run yet. The window never runs checks by itself.</summary>
        public void ShowIdle()
        {
            _results.Clear();
            Verdict(null, Messages.Get("ui.verdict.idle.title"), Messages.Get("ui.verdict.idle.sub"), 0, true);
        }

        public void Show(IList<Finding> findings)
        {
            _results.Clear();
            var errors = findings.Where(f => f.Severity == Severity.Error).ToList();
            var warnings = findings.Where(f => f.Severity == Severity.Warning).ToList();
            var notes = findings.Where(f => f.Severity == Severity.Info).ToList();
            int oneClick = FixFlow.CountAllInOne(findings);

            string title, sub = "";
            Severity? icon;
            if (errors.Count > 0)
            {
                icon = Severity.Error;
                title = errors.Count == 1 ? Messages.Get("ui.verdict.error.one") : Messages.Get("ui.verdict.error.many", errors.Count);
                sub = oneClick > 0 ? Messages.Get("ui.verdict.fixable", oneClick) : Messages.Get("ui.verdict.manual");
                if (warnings.Count > 0) sub += " " + Messages.Get("ui.verdict.alsoWarnings", warnings.Count);
            }
            else if (warnings.Count > 0)
            {
                icon = Severity.Warning;
                title = Messages.Get("ui.verdict.warn", warnings.Count);
                if (oneClick > 0) sub = Messages.Get("ui.verdict.fixable", oneClick);
            }
            else
            {
                icon = null;
                title = Messages.Get("ui.verdict.ok.title");
                sub = notes.Count > 0 ? Messages.Get("ui.verdict.ok.notes", notes.Count) : Messages.Get("ui.verdict.ok.none");
            }
            Verdict(icon, title, sub, oneClick, false);

            AddGroup(Messages.Get("ui.group.errors", errors.Count), errors, false);
            AddGroup(Messages.Get("ui.group.warnings", warnings.Count), warnings, false);
            AddGroup(Messages.Get("ui.group.notes", notes.Count), notes, true);
        }

        /// <summary>One message for a whole run that failed, with a button to copy the exception.</summary>
        public void ShowInternalError(Exception e)
        {
            _results.Clear();
            Verdict(Severity.Error, Messages.Get("ui.error.title"), e.GetType().Name + ": " + e.Message, 0, false);
            _results.Add(new Label(e.GetType().Name + ": " + e.Message));
            _results.Add(new Button(() => UnityEditor.EditorGUIUtility.systemCopyBuffer = e.ToString())
            { text = Messages.Get("ui.error.copy") });
        }

        /// <summary>Tells which APK or AAB the build checks look at, or hides the line when there is none.</summary>
        public void ShowBuildFile(string path)
        {
            _buildFile.style.display = string.IsNullOrEmpty(path) ? DisplayStyle.None : DisplayStyle.Flex;
            _buildFile.text = string.IsNullOrEmpty(path) ? "" : Messages.Get("ui.build.current", path);
        }

        void Verdict(Severity? severity, string title, string sub, int oneClick, bool idle)
        {
            _verdictIcon.image = severity.HasValue ? UiStyles.Icon(severity.Value, true) : (idle ? null : UiStyles.PassedIcon());
            _verdictIcon.style.display = _verdictIcon.image == null ? DisplayStyle.None : DisplayStyle.Flex;
            _verdictTitle.text = title;
            _verdictSub.text = sub;
            _verdictSub.style.display = string.IsNullOrEmpty(sub) ? DisplayStyle.None : DisplayStyle.Flex;

            _fixAllButton.text = Messages.Get("ui.fixAll", oneClick);
            _fixAllButton.style.display = oneClick > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _fixAllButton.EnableInClassList("ad-primary", true);

            _runButton.text = Messages.Get(idle ? "ui.checkProject" : "ui.checkAgain");
            // One strong button at a time: Fix all when there is something to fix, otherwise the check itself.
            bool runIsPrimary = idle || oneClick == 0;
            _runButton.EnableInClassList("ad-primary", runIsPrimary);
            _runButton.EnableInClassList("ad-secondary", !runIsPrimary);
        }

        void AddGroup(string title, List<Finding> findings, bool collapsed)
        {
            if (findings.Count == 0) return;
            if (collapsed)
            {
                var foldout = new Foldout { text = title, value = false };
                foldout.AddToClassList("ad-group-title");
                foreach (var finding in findings) foldout.Add(new FindingCard(finding, _onFix, _notify));
                _results.Add(foldout);
                return;
            }
            var label = new Label(title);
            label.AddToClassList("ad-group-title");
            _results.Add(label);
            foreach (var finding in findings) _results.Add(new FindingCard(finding, _onFix, _notify));
        }
    }
}
