using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndroidDoctor.UI
{
    /// <summary>One finding as a card: severity colour, title, detail, folded evidence and the buttons that apply.</summary>
    public sealed class FindingCard : VisualElement
    {
        public static readonly Color ErrorColor = new Color32(0xD9, 0x53, 0x4F, 0xFF);
        public static readonly Color WarningColor = new Color32(0xE0, 0xA8, 0x00, 0xFF);
        public static readonly Color InfoColor = new Color32(0x5B, 0x8D, 0xEF, 0xFF);

        public Finding Finding { get; }
        public Label IdLabel { get; }
        public Label TitleLabel { get; }
        public Label DetailLabel { get; }
        public Foldout EvidenceFoldout { get; }   // null when the finding has no evidence
        public Button FixButton { get; }          // the first action's button; null unless there is one
        public IReadOnlyList<Button> ActionButtons { get; } // one per action whose fix exists
        public Button CopyCommandButton { get; }  // null unless the finding has a command
        public Button DocsButton { get; }         // null unless the finding has a docs URL

        public static Color ColorFor(Severity severity)
        {
            switch (severity)
            {
                case Severity.Error: return ErrorColor;
                case Severity.Warning: return WarningColor;
                default: return InfoColor;
            }
        }

        /// <param name="onFix">Called when the user presses the fix button. May be null.</param>
        /// <param name="notify">Shows a short message to the user. May be null.</param>
        public FindingCard(Finding finding, Action<Finding, FixAction> onFix, Action<string> notify = null)
        {
            Finding = finding;
            style.borderLeftWidth = 4;
            style.borderLeftColor = ColorFor(finding.Severity);
            style.marginBottom = 6;
            style.paddingLeft = 8;
            style.paddingRight = 6;
            style.paddingTop = 4;
            style.paddingBottom = 4;
            style.backgroundColor = new Color(1f, 1f, 1f, 0.05f);

            // The rule id in grey, then the title in bold.
            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            IdLabel = new Label(finding.RuleId);
            IdLabel.style.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            IdLabel.style.marginRight = 6;
            IdLabel.style.flexShrink = 0;
            TitleLabel = new Label(finding.Title);
            TitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            TitleLabel.style.whiteSpace = WhiteSpace.Normal;
            TitleLabel.style.flexGrow = 1;
            TitleLabel.style.flexShrink = 1;
            TitleLabel.style.flexBasis = 0;
            titleRow.Add(IdLabel);
            titleRow.Add(TitleLabel);
            Add(titleRow);

            DetailLabel = new Label(finding.Detail);
            DetailLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(DetailLabel);

            if (finding.Evidence.Count > 0)
            {
                // A read-only multiline field, so paths and versions can be selected and copied.
                EvidenceFoldout = new Foldout { text = Messages.Get("ui.evidence"), value = false };
                var text = new TextField { multiline = true, isReadOnly = true, value = string.Join("\n", finding.Evidence) };
                EvidenceFoldout.Add(text);
                Add(EvidenceFoldout);
            }

            var actions = finding.Actions.Where(a => FixRegistry.Get(a.FixId) != null).ToList();
            var actionButtons = new List<Button>();
            ActionButtons = actionButtons;
            if (actions.Count == 0 && string.IsNullOrEmpty(finding.Command) && string.IsNullOrEmpty(finding.DocsUrl)) return;

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.marginTop = 2;
            buttons.style.flexWrap = Wrap.Wrap;
            foreach (var action in actions)
            {
                var button = new Button(() => onFix?.Invoke(finding, action)) { text = action.Label ?? Messages.Get("ui.fix") };
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
                buttons.Add(CopyCommandButton);
            }
            if (!string.IsNullOrEmpty(finding.DocsUrl))
            {
                DocsButton = new Button(() => Application.OpenURL(finding.DocsUrl)) { text = Messages.Get("ui.docs") };
                buttons.Add(DocsButton);
            }
            Add(buttons);
        }
    }
}
