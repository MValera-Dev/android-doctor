using System.Collections.Generic;
using UnityEditor;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV001: the Android module is installed in this editor.</summary>
    public sealed class AndroidModuleInstalledRule : IRule
    {
        public string Id => "ENV001";
        public Category Category => Category.Environment;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            return Evaluate(ctx, BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android));
        }

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, bool androidSupported)
        {
            if (androidSupported) yield break;
            yield return new Finding("ENV001", Category.Environment, Severity.Error,
                Messages.Get("ENV001.title"), Messages.Get("ENV001.detail"),
                new[] { Messages.Get("ENV001.evidence", ctx.Unity) });
        }
    }
}
