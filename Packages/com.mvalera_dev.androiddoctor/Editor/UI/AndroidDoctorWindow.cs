using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndroidDoctor.UI
{
    /// <summary>
    /// Window > Android Doctor. The Checks tab looks at the project and the last build and offers the fixes; the
    /// Gradle log tab explains a log you paste. Nothing runs until the user asks.
    /// </summary>
    public sealed class AndroidDoctorWindow : EditorWindow
    {
        static readonly TimeSpan ScanTimeBudget = TimeSpan.FromSeconds(30);

        Button _checksTab, _gradleTab;
        ChecksPanel _checks;
        GradleLogPanel _gradle;

        // Null until the first run: Fix all and Copy report need a run to have happened.
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
            UiStyles.Apply(root);
            root.AddToClassList("ad-root");

            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(AndroidDoctorWindow).Assembly);
            var header = new VisualElement();
            header.AddToClassList("ad-header");
            var title = new Label(Messages.Get("ui.window.title"));
            title.AddToClassList("ad-title");
            var version = new Label(package != null ? package.version : "dev");
            version.AddToClassList("ad-version");
            version.AddToClassList("ad-muted");
            header.Add(title);
            header.Add(version);
            root.Add(header);

            var tabs = new VisualElement();
            tabs.AddToClassList("ad-tabs");
            _checksTab = new Button(() => SelectTab(true)) { text = Messages.Get("ui.tab.checks") };
            _gradleTab = new Button(() => SelectTab(false)) { text = Messages.Get("ui.tab.gradle") };
            _checksTab.AddToClassList("ad-tab");
            _gradleTab.AddToClassList("ad-tab");
            tabs.Add(_checksTab);
            tabs.Add(_gradleTab);
            root.Add(tabs);

            _checks = new ChecksPanel(ApplyFix, Notify, RunChecks, FixAll, ScanBuild, CopyReport);
            _gradle = new GradleLogPanel(Notify, ApplyFix);
            root.Add(_checks);
            root.Add(_gradle);
            SelectTab(true);
        }

        void Notify(string message) => ShowNotification(new GUIContent(message));

        void SelectTab(bool checks)
        {
            _checks.style.display = checks ? DisplayStyle.Flex : DisplayStyle.None;
            _gradle.style.display = checks ? DisplayStyle.None : DisplayStyle.Flex;
            _checksTab.EnableInClassList("ad-tab--selected", checks);
            _gradleTab.EnableInClassList("ad-tab--selected", !checks);
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
                _checks.ShowBuildFile(ctx.ArtifactPath);
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

        void FixAll()
        {
            string message;
            bool applied = FixFlow.RunAll(_lastFindings, out message);
            if (message != null) Notify(message);
            if (applied) RunChecks();
        }

        void ApplyFix(Finding finding, FixAction action)
        {
            string message;
            bool applied = FixFlow.Run(finding, action, out message);
            if (message != null) Notify(message);
            if (applied) RunChecks(); // run again so the finding disappears
        }
    }
}
