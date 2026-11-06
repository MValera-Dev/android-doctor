using System.Collections.Generic;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV005 (informational): the build-tools revision Unity ships with is installed.</summary>
    public sealed class BuildToolsRule : IRule
    {
        public string Id => "ENV005";
        public Category Category => Category.Environment;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx);

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx)
        {
            var sdk = ctx.Tools == null ? null : ctx.Tools.Sdk;
            if (sdk == null || string.IsNullOrEmpty(sdk.Path) || !sdk.Exists) yield break;
            var row = ctx.Compat == null ? null : ctx.Compat.Find(ctx.Unity);
            if (row == null || string.IsNullOrEmpty(row.buildTools)) yield break;

            var installed = AndroidSdkLayout.InstalledBuildTools(sdk.Path);
            if (installed.Contains(row.buildTools)) yield break;

            string installedText = installed.Count == 0
                ? Messages.Get("ENV005.evidence.none")
                : string.Join(", ", installed);
            yield return new Finding("ENV005", Category.Environment, Severity.Info,
                Messages.Get("ENV005.title", row.buildTools),
                Messages.Get("ENV005.detail", ctx.Unity.Key, row.buildTools, installedText),
                new[] { Messages.Get("ENV005.evidence.path", sdk.Path) });
        }
    }
}
