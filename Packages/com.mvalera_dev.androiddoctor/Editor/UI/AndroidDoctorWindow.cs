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

            var run = new Button(RunChecks) { text = Messages.Get("ui.run") };
            run.style.marginTop = 6;
            run.style.marginBottom = 6;
            root.Add(run);

            _summary = new Label(Messages.Get("ui.idle"));
            root.Add(_summary);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            _results = scroll;
            root.Add(scroll);
        }

        void RunChecks()
        {
            _results.Clear();
            try
            {
                var ctx = RuleContext.Create();
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
                _results.Add(new Label(f.RuleId + "  " + f.Title));
            }
            _summary.text = findings.Count == 0
                ? Messages.Get("ui.noFindings")
                : Messages.Get("ui.summary", errors, warnings, infos);
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
