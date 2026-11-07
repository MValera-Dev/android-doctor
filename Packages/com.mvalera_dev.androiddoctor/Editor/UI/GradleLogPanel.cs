using System;
using UnityEditor;
using UnityEngine.UIElements;
using AndroidDoctor.Gradle;

namespace AndroidDoctor.UI
{
    /// <summary>The Gradle log tab: paste a log or read the last build, and get the probable cause of the failure.</summary>
    public sealed class GradleLogPanel : VisualElement
    {
        readonly TextField _input;
        readonly Label _message;
        readonly VisualElement _results;
        readonly Action<string> _notify;
        readonly Action<Finding, FixAction> _onFix;

        public TextField Input => _input;
        public VisualElement Results => _results;
        public string MessageText => _message.text;

        /// <param name="onFix">Called when a fix button of a finding is pressed. May be null.</param>
        public GradleLogPanel(Action<string> notify, Action<Finding, FixAction> onFix = null)
        {
            _notify = notify;
            _onFix = onFix;
            UiStyles.Apply(this);
            style.flexGrow = 1;

            var hint = new Label(Messages.Get("ui.gradle.hint"));
            hint.AddToClassList("ad-hint");
            Add(hint);

            _input = new TextField { multiline = true };
            _input.AddToClassList("ad-log-input");
            Add(_input);

            var buttons = new VisualElement();
            buttons.AddToClassList("ad-button-row");
            buttons.Add(new Button(AnalyzePasted) { text = Messages.Get("ui.gradle.analyzePasted") });
            buttons.Add(new Button(AnalyzeLastBuild) { text = Messages.Get("ui.gradle.analyzeLast") });
            buttons.Add(new Button(UseCaptured) { text = Messages.Get("ui.gradle.useCaptured") });
            Add(buttons);

            _message = new Label();
            _message.AddToClassList("ad-message");
            Add(_message);

            var scroll = new ScrollView();
            scroll.AddToClassList("ad-results");
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _results = scroll;
            Add(scroll);
        }

        public void AnalyzePasted() => Show(GradleLogParser.Extract(_input.value), "ui.gradle.notFound");

        public void AnalyzeLastBuild() => Show(LastBuildAnalyzer.Analyze(), "ui.gradle.noLastBuild");

        public void UseCaptured() => Show(GradleErrorCapture.Get(), "ui.gradle.noCaptured");

        /// <param name="missingKey">The text to show when there is no failure.</param>
        public void Show(GradleError error, string missingKey)
        {
            _results.Clear();
            if (error == null)
            {
                _message.text = Messages.Get(missingKey);
                return;
            }
            _message.text = "";

            // The probable causes first, as cards. They never have fix buttons: Gradle problems are fixed by hand.
            foreach (var finding in GradleTranslator.Translate(error))
                _results.Add(new FindingCard(finding, _onFix, _notify));

            // The original block is always there, folded, in case nothing matched or the explanation is not enough.
            var original = new Foldout { text = Messages.Get("ui.gradle.original"), value = false };
            original.Add(new TextField { multiline = true, isReadOnly = true, value = error.Block });
            _results.Add(original);

            _results.Add(new Button(() =>
            {
                EditorGUIUtility.systemCopyBuffer = ForumReport.Build(error);
                _notify?.Invoke(Messages.Get("ui.gradle.copyForum.done"));
            }) { text = Messages.Get("ui.gradle.copyForum") });
        }
    }
}
