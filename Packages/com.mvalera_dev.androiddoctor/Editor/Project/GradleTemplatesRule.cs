using System.Collections.Generic;
using System.IO;

namespace AndroidDoctor.Project
{
    /// <summary>PRJ005: custom Gradle templates that replace Unity's defaults and can break after an upgrade.</summary>
    public sealed class GradleTemplatesRule : IRule
    {
        static readonly string[] TemplateFiles =
        {
            "mainTemplate.gradle", "launcherTemplate.gradle", "baseProjectTemplate.gradle",
            "settingsTemplate.gradle", "gradleTemplate.properties",
        };

        public string Id => "PRJ005";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx);

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx)
        {
            if (string.IsNullOrEmpty(ctx.AssetsPath)) yield break;

            var found = new List<string>();
            foreach (string file in TemplateFiles)
                if (File.Exists(Path.Combine(ctx.AssetsPath, "Plugins", "Android", file)))
                    found.Add("Assets/Plugins/Android/" + file);
            if (found.Count == 0) yield break;

            // From Unity 6.5 the Android build engine moved to Gradle 9 / AGP 9, which breaks old templates.
            var severity = ctx.Unity.AtLeast(6000, 5) ? Severity.Warning : Severity.Info;
            var row = ctx.Compat == null ? null : ctx.Compat.Find(ctx.Unity);
            bool versionsKnown = row != null && !string.IsNullOrEmpty(row.gradle) && !string.IsNullOrEmpty(row.agp);
            string detail = versionsKnown
                ? Messages.Get("PRJ005.detail.versions", ctx.Unity.Key, row.gradle, row.agp)
                : Messages.Get("PRJ005.detail.generic");

            yield return new Finding("PRJ005", Category.Project, severity,
                Messages.Get("PRJ005.title"), detail, found,
                actions: new[] { new FixAction(FixIds.ResetTemplates, Messages.Get("PRJ005.fix"), string.Join(ResetTemplatesFix.Separator.ToString(), found)) });
        }
    }
}
