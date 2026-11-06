using System.Collections.Generic;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>PRJ002: the minimum API level meets the minimum this Unity version requires.</summary>
    public sealed class MinApiRule : IRule
    {
        public string Id => "PRJ002";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            return Evaluate(ctx, (int)PlayerSettings.Android.minSdkVersion);
        }

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, int configuredMin)
        {
            var row = ctx.Compat == null ? null : ctx.Compat.Find(ctx.Unity);
            if (row == null || row.minSdk < 0 || configuredMin >= row.minSdk) yield break;

            yield return new Finding("PRJ002", Category.Project, Severity.Error,
                Messages.Get("PRJ002.title"), Messages.Get("PRJ002.detail", ctx.Unity.Key, row.minSdk),
                new[]
                {
                    Messages.Get("PRJ002.evidence.min", configuredMin),
                    Messages.Get("PRJ002.evidence.unityMin", row.minSdk),
                },
                FixIds.MinApi, Messages.Get("PRJ002.fix", row.minSdk));
        }
    }
}
