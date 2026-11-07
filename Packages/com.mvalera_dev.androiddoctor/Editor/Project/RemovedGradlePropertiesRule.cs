using System.Collections.Generic;
using System.IO;

namespace AndroidDoctor.Project
{
    /// <summary>
    /// PRJ010: gradleTemplate.properties still sets a property that the Android Gradle plugin in Unity 6 no longer accepts.
    /// The build fails with "Failed to apply plugin", so it is found before the build instead of after it.
    /// </summary>
    public sealed class RemovedGradlePropertiesRule : IRule
    {
        public string Id => "PRJ010";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx);

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx)
        {
            if (string.IsNullOrEmpty(ctx.AssetsPath) || !ctx.Unity.AtLeast(6000, 0)) yield break;
            string file = Path.Combine(ctx.AssetsPath, "Plugins", "Android", "gradleTemplate.properties");
            if (!File.Exists(file)) yield break;

            var removed = GradleProperties.FindRemoved(File.ReadAllText(file));
            if (removed.Count == 0) yield break;

            yield return new Finding("PRJ010", Category.Project, Severity.Error,
                Messages.Get("PRJ010.title"), Messages.Get("PRJ010.detail"), removed,
                actions: new[]
                {
                    new FixAction(FixIds.GradleProperties, Messages.Get("PRJ010.fix"), GradlePropertiesFix.TemplatePath, true),
                    new FixAction(FixIds.ResetTemplates, Messages.Get("PRJ010.reset"), GradlePropertiesFix.TemplatePath),
                });
        }
    }
}
