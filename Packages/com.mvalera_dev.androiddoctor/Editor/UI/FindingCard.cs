using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndroidDoctor.UI
{
    /// <summary>
    /// One finding as a flat row: the Console's icon for its severity, the problem in bold, what to do about it, the
    /// evidence folded away, and one button per thing the user can press. Styling is in AndroidDoctor.uss.
    /// </summary>
    public sealed class FindingCard : VisualElement
    {
        public Finding Finding { get; }
        public Label IdLabel { get; }
        public Label TitleLabel { get; }
        public Label DetailLabel { get; }
        public Foldout EvidenceFoldout { get; }   // null when the finding has no evidence
        public Button FixButton { get; }          // the first action's button; null unless there is one
        public IReadOnlyList<Button> ActionButtons { get; } // one per action whose fix exists
        public Button CopyCommandButton { get; }  // null unless the finding has a command
        public Button DocsButton { get; }         // null unless the finding has a docs URL

        /// <param name="onFix">Called when the user presses one of the fix buttons. May be null.</param>
        /// <param name="notify">Shows a short message to the user. May be null.</param>
        public FindingCard(Finding finding, Action<Finding, FixAction> onFix, Action<string> notify = null)
        {
            Finding = finding;
            AddToClassList("ad-row");
            AddToClassList(UiStyles.ClassFor(finding.Severity));

            var icon = new Image { image = UiStyles.Icon(finding.Severity), scaleMode = ScaleMode.ScaleToFit };
            icon.AddToClassList("ad-row-icon");
            Add(icon);

            var body = new VisualElement();
            body.AddToClassList("ad-row-body");
            Add(body);

            var head = new VisualElement();
            head.AddToClassList("ad-row-head");
            TitleLabel = new Label(finding.Title);
            TitleLabel.AddToClassList("ad-row-title");
            IdLabel = new Label(finding.RuleId);
            IdLabel.AddToClassList("ad-row-id");
            IdLabel.AddToClassList("ad-muted");
            head.Add(TitleLabel);
            head.Add(IdLabel);
            body.Add(head);

            DetailLabel = new Label(finding.Detail);
            DetailLabel.AddToClassList("ad-row-detail");
            body.Add(DetailLabel);

            if (finding.Evidence.Count > 0)
            {
                // A read-only multiline field, so paths and versions can be selected and copied.
                EvidenceFoldout = new Foldout { text = Messages.Get("ui.evidence"), value = false };
                EvidenceFoldout.AddToClassList("ad-row-evidence");
                EvidenceFoldout.Add(new TextField { multiline = true, isReadOnly = true, value = string.Join("\n", finding.Evidence) });
                body.Add(EvidenceFoldout);
            }

            var actions = finding.Actions.Where(a => FixRegistry.Get(a.FixId) != null).ToList();
            var actionButtons = new List<Button>();
            ActionButtons = actionButtons;
            if (actions.Count == 0 && string.IsNullOrEmpty(finding.Command) && string.IsNullOrEmpty(finding.DocsUrl)) return;

            var buttons = new VisualElement();
            buttons.AddToClassList("ad-row-buttons");
            foreach (var action in actions)
            {
                var button = new Button(() => onFix?.Invoke(finding, action)) { text = action.Label ?? Messages.Get("ui.fix") };
                button.AddToClassList("ad-action");
                if (action.AllInOne) button.AddToClassList("ad-action--recommended"); // the one-click answer
                actionButtons.Add(button);
                buttons.Add(button);
            }
            FixButton = actionButtons.FirstOrDefault();

            if (!string.IsNullOrEmpty(finding.Command))
            {
                CopyCommandButton = new Button(() =>
                {
                    EditorGUIUtility.systemCopyBuffer = finding.Command;
                    notify?.Invoke(Messages.Get("ui.copyCommand.done"));
                }) { text = Messages.Get("ui.copyCommand") };
                CopyCommandButton.AddToClassList("ad-action");
                buttons.Add(CopyCommandButton);
            }
            if (!string.IsNullOrEmpty(finding.DocsUrl))
            {
                DocsButton = new Button(() => Application.OpenURL(finding.DocsUrl)) { text = Messages.Get("ui.docs") };
                DocsButton.AddToClassList("ad-action");
                buttons.Add(DocsButton);
            }
            body.Add(buttons);
        }
    }
}
