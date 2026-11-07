using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using AndroidDoctor.Project;

namespace AndroidDoctor.UI
{
    /// <summary>
    /// Window > Android Doctor. Two tabs: Checks (run the rules, read the findings, fix, scan a build, copy a report)
    /// and Gradle log (explain a failed build). Nothing runs until the user asks.
    /// </summary>
    public sealed class AndroidDoctorWindow : EditorWindow
    {
        static readonly TimeSpan ScanTimeBudget = TimeSpan.FromSeconds(30);

        Label _subheader;
        Label _buildFile;
        Button _checksTab, _gradleTab;
        VisualElement _checksToolbar;
        ChecksPanel _checks;
        GradleLogPanel _gradle;

        // Null until the first run: Copy report needs a run to have happened.
        List<Finding> _lastFindings;

        [MenuItem("Window/Android Doctor")]
        public static void Open()
        {
            var window = GetWindow<AndroidDoctorWindow>();
            window.titleContent = new GUIContent(Messages.Get("ui.window.title"));
            window.minSize = new Vector2(460, 360);
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
            _subheader = new Label();
            root.Add(_subheader);
            RefreshSubheader();

            // Tabs.
            var tabs = new VisualElement();
            tabs.style.flexDirection = FlexDirection.Row;
            tabs.style.marginTop = 6;
            _checksTab = new Button(() => SelectTab(true)) { text = Messages.Get("ui.tab.checks") };
            _gradleTab = new Button(() => SelectTab(false)) { text = Messages.Get("ui.tab.gradle") };
            _checksTab.style.flexGrow = 1;
            _gradleTab.style.flexGrow = 1;
            tabs.Add(_checksTab);
            tabs.Add(_gradleTab);
            root.Add(tabs);

            // Checks toolbar: Run checks, Scan build, Copy report.
            _checksToolbar = new VisualElement();
            _checksToolbar.style.flexDirection = FlexDirection.Row;
            _checksToolbar.style.marginTop = 4;
            _checksToolbar.Add(Grow(new Button(RunChecks) { text = Messages.Get("ui.run") }));
            _checksToolbar.Add(Grow(new Button(ScanBuild) { text = Messages.Get("ui.scanBuild") }));
            _checksToolbar.Add(Grow(new Button(CopyReport) { text = Messages.Get("ui.copyReport") }));
            root.Add(_checksToolbar);

            _buildFile = new Label();
            _buildFile.style.display = DisplayStyle.None;
            root.Add(_buildFile);

            _checks = new ChecksPanel(ApplyFix, Notify);
            _gradle = new GradleLogPanel(Notify);
            root.Add(_checks);
            root.Add(_gradle);
            SelectTab(true);
        }

        static Button Grow(Button b) { b.style.flexGrow = 1; return b; }

        void Notify(string message) => ShowNotification(new GUIContent(message));

        void SelectTab(bool checks)
        {
            _checks.style.display = checks ? DisplayStyle.Flex : DisplayStyle.None;
            _checksToolbar.style.display = checks ? DisplayStyle.Flex : DisplayStyle.None;
            _gradle.style.display = checks ? DisplayStyle.None : DisplayStyle.Flex;
            _checksTab.style.unityFontStyleAndWeight = checks ? FontStyle.Bold : FontStyle.Normal;
            _gradleTab.style.unityFontStyleAndWeight = checks ? FontStyle.Normal : FontStyle.Bold;
        }

        void RefreshSubheader()
        {
            _subheader.text = Messages.Get("ui.subheader", Application.unityVersion,
                TargetApiResolver.Format((int)PlayerSettings.Android.targetSdkVersion));
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
                        (progress, ruleId) => EditorUtility.DisplayProgressBar(Messages.Get("ui.progress.title"), ruleId, progress),
                        ScanTimeBudget);
                }
                finally { EditorUtility.ClearProgressBar(); } // an exception must never leave the bar stuck

                _lastFindings = findings;
                RefreshSubheader();
                _checks.Show(findings);
            }
            catch (Exception e)
            {
                _lastFindings = null;
                _checks.ShowInternalError(e);
            }
        }

        void CopyReport()
        {
            if (_lastFindings == null) { Notify(Messages.Get("ui.copyReport.first")); return; }
            try
            {
                // Always English: the findings on screen are in the UI language, so the checks run again for the report.
                EditorGUIUtility.systemCopyBuffer = EnglishReport.BuildCurrent();
                Notify(Messages.Get("ui.copyReport.done"));
            }
            catch (Exception e) { _checks.ShowInternalError(e); }
        }

        void ApplyFix(Finding finding)
        {
            string message;
            bool applied = FixFlow.Run(finding, out message);
            if (message != null) Notify(message);
            if (applied) RunChecks(); // run again so the finding disappears
        }
    }
}
