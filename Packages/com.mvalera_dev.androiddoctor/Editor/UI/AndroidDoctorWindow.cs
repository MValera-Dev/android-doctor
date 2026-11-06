using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndroidDoctor.UI
{
    /// <summary>Minimal window: a Run checks button and one label per finding (rule id + title).</summary>
    public sealed class AndroidDoctorWindow : EditorWindow
    {
        Label _summary;
        Label _buildFile;
        VisualElement _results;

        [MenuItem("Window/Android Doctor")]
        public static void Open()
        {
            var window = GetWindow<AndroidDoctorWindow>();
            window.titleContent = new GUIContent(Messages.Get("ui.window.title"));
            window.minSize = new Vector2(420, 300);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = root.style.paddingRight = 8;
            root.style.paddingTop = root.style.paddingBottom = 8;

            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(AndroidDoctorWindow).Assembly);
            var header = new Label(Messages.Get("ui.header", package != null ? package.version : "dev"));
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.fontSize = 14;
            root.Add(header);
            root.Add(new Label(Messages.Get("ui.subheader", Application.unityVersion)));

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.marginTop = 6;
            buttons.style.marginBottom = 6;
            var run = new Button(RunChecks) { text = Messages.Get("ui.run") };
            run.style.flexGrow = 1;
            var scanBuild = new Button(ScanBuild) { text = Messages.Get("ui.scanBuild") };
            scanBuild.style.flexGrow = 1;
            buttons.Add(run);
            buttons.Add(scanBuild);
            root.Add(buttons);

            _buildFile = new Label();
            _buildFile.style.display = DisplayStyle.None;
            root.Add(_buildFile);

            _summary = new Label(Messages.Get("ui.idle"));
            root.Add(_summary);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _results = scroll;
            root.Add(scroll);
        }

        // Lets the user pick an APK or AAB by hand; the checks then include the build libraries (BLD001).
        void ScanBuild()
        {
            string path = EditorUtility.OpenFilePanel(Messages.Get("ui.scanBuild.panel"), "", "apk,aab");
            if (string.IsNullOrEmpty(path)) return; // cancelled
            Build.BuildArtifactState.Set(path);
            RunChecks();
        }

        void RunChecks()
        {
            _results.Clear();
            try
            {
                var ctx = RuleContext.Create();
                _buildFile.style.display = ctx.ArtifactPath == null ? DisplayStyle.None : DisplayStyle.Flex;
                if (ctx.ArtifactPath != null) _buildFile.text = Messages.Get("ui.build.current", ctx.ArtifactPath);
                var rules = RuleRunner.DiscoverRules();
                List<Finding> findings;
                try
                {
                    findings = RuleRunner.RunAll(rules, ctx,
                        (progress, ruleId) => EditorUtility.DisplayProgressBar(Messages.Get("ui.progress.title"), ruleId, progress));
                }
                finally { EditorUtility.ClearProgressBar(); }
                ShowFindings(findings);
            }
            catch (Exception e) { ShowInternalError(e); }
        }

        void ShowFindings(List<Finding> findings)
        {
            int errors = 0, warnings = 0, infos = 0;
            foreach (var f in findings)
            {
                if (f.Severity == Severity.Error) errors++;
                else if (f.Severity == Severity.Warning) warnings++;
                else infos++;
                _results.Add(CreateRow(f));
            }
            _summary.text = findings.Count == 0
                ? Messages.Get("ui.noFindings")
                : Messages.Get("ui.summary", errors, warnings, infos);
        }

        VisualElement CreateRow(Finding finding)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            // The label takes the free width and wraps; the button keeps its size instead of being pushed out.
            var label = new Label(finding.RuleId + "  " + finding.Title);
            label.style.flexGrow = 1;
            label.style.flexShrink = 1;
            label.style.flexBasis = 0;
            label.style.whiteSpace = WhiteSpace.Normal;
            row.Add(label);

            if (!string.IsNullOrEmpty(finding.FixId) && FixRegistry.Get(finding.FixId) != null)
            {
                var button = new Button(() => ApplyFix(finding)) { text = finding.FixLabel ?? Messages.Get("ui.fix") };
                button.style.flexShrink = 0;
                row.Add(button);
            }
            return row;
        }

        void ApplyFix(Finding finding)
        {
            string message;
            bool applied = FixFlow.Run(finding, out message);
            if (message != null) ShowNotification(new GUIContent(message));
            if (applied) RunChecks(); // run again so the finding disappears
        }

        void ShowInternalError(Exception e)
        {
            _summary.text = Messages.Get("ui.error.title");
            _results.Add(new Label(e.GetType().Name + ": " + e.Message));
            _results.Add(new Button(() => EditorGUIUtility.systemCopyBuffer = e.ToString())
            { text = Messages.Get("ui.error.copy") });
        }
    }
}
