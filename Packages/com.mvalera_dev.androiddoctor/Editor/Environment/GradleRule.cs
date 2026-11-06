using System.Collections.Generic;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV007: a custom Gradle exists and matches the version Unity expects (when known).</summary>
    public sealed class GradleRule : IRule
    {
        public string Id => "ENV007";
        public Category Category => Category.Environment;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx);

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx)
        {
            var gradle = ctx.Tools == null ? null : ctx.Tools.Gradle;
            // The Gradle that ships with Unity is Unity's own choice: only judge a custom one.
            if (gradle == null || gradle.Source != ToolSource.Custom || string.IsNullOrEmpty(gradle.Path)) yield break;

            if (!gradle.Exists)
            {
                yield return new Finding("ENV007", Category.Environment, Severity.Warning,
                    Messages.Get("ENV007.missing.title"), Messages.Get("ENV007.missing.detail"),
                    new[] { Messages.Get("ENV007.evidence.path", gradle.Path) });
                yield break;
            }

            var row = ctx.Compat == null ? null : ctx.Compat.Find(ctx.Unity);
            // Gradle changes between patch releases of one Unity minor, so many rows leave it unknown.
            if (row == null || string.IsNullOrEmpty(row.gradle)) yield break;

            string version = GradleVersion.FromLauncherJar(gradle.Path);
            if (version == null || version == row.gradle) yield break;

            yield return new Finding("ENV007", Category.Environment, Severity.Warning,
                Messages.Get("ENV007.version.title"),
                Messages.Get("ENV007.version.detail", ctx.Unity.Key, row.gradle, version),
                new[] { Messages.Get("ENV007.evidence.path", gradle.Path), Messages.Get("ENV007.evidence.version", version) });
        }
    }
}
