using System.Collections.Generic;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV002: the JDK exists and, if it is a custom one, matches the major version Unity expects.</summary>
    public sealed class JdkRule : IRule
    {
        public string Id => "ENV002";
        public Category Category => Category.Environment;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx);

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx)
        {
            var jdk = ctx.Tools == null ? null : ctx.Tools.Jdk;
            if (jdk == null || string.IsNullOrEmpty(jdk.Path)) yield break;

            if (!jdk.Exists)
            {
                yield return new Finding("ENV002", Category.Environment, Severity.Error,
                    Messages.Get("ENV002.missing.title"), Messages.Get("ENV002.missing.detail"),
                    new[] { Messages.Get("ENV002.evidence.path", jdk.Path) },
                    actions: UseUnityToolsFix.ActionsFor("jdk", jdk.Source == ToolSource.Custom));
                yield break;
            }

            // The JDK that ships with Unity is Unity's own choice: only judge a custom one.
            if (jdk.Source != ToolSource.Custom) yield break;
            var row = ctx.Compat == null ? null : ctx.Compat.Find(ctx.Unity);
            if (row == null || row.jdk < 0) yield break;

            string version = JdkRelease.ReadVersion(jdk.Path);
            int major = JdkRelease.ParseMajor(version);
            if (major < 0 || major == row.jdk) yield break;

            yield return new Finding("ENV002", Category.Environment, Severity.Warning,
                Messages.Get("ENV002.version.title"),
                Messages.Get("ENV002.version.detail", ctx.Unity.Key, row.jdk, version),
                new[] { Messages.Get("ENV002.evidence.path", jdk.Path), Messages.Get("ENV002.evidence.version", version) },
                actions: UseUnityToolsFix.ActionsFor("jdk", true));
        }
    }
}
