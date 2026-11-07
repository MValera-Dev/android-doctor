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

        public TextField Input => _input;
        public VisualElement Results => _results;
        public string MessageText => _message.text;

        public GradleLogPanel(Action<string> notify)
        {
            _notify = notify;
            style.flexGrow = 1;

            var hint = new Label(Messages.Get("ui.gradle.hint"));
            hint.style.whiteSpace = WhiteSpace.Normal;
            hint.style.marginTop = 4;
            Add(hint);

            _input = new TextField { multiline = true };
            _input.style.minHeight = 100;
            _input.style.maxHeight = 160;
            _input.style.marginTop = 4;
            Add(_input);

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.marginTop = 4;
            buttons.style.marginBottom = 4;
            buttons.Add(Grow(new Button(AnalyzePasted) { text = Messages.Get("ui.gradle.analyzePasted") }));
            buttons.Add(Grow(new Button(AnalyzeLastBuild) { text = Messages.Get("ui.gradle.analyzeLast") }));
            buttons.Add(Grow(new Button(UseCaptured) { text = Messages.Get("ui.gradle.useCaptured") }));
            Add(buttons);

            _message = new Label();
            _message.style.whiteSpace = WhiteSpace.Normal;
            Add(_message);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _results = scroll;
            Add(scroll);
        }

        static Button Grow(Button b) { b.style.flexGrow = 1; return b; }

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
                _results.Add(new FindingCard(finding, null, _notify));

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
