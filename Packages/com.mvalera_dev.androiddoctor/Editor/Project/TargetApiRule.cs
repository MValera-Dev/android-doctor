using System.Collections.Generic;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>PRJ001: the target API level meets the Google Play minimum.</summary>
    public sealed class TargetApiRule : IRule
    {
        public string Id => "PRJ001";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            return Evaluate(ctx, (int)PlayerSettings.Android.targetSdkVersion);
        }

        /// <param name="configuredTarget">The configured target API level; 0 or less means Auto.</param>
        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, int configuredTarget)
        {
            if (ctx.Play == null || ctx.Play.minTargetSdk <= 0) yield break;
            int minimum = ctx.Play.minTargetSdk;

            int effective = TargetApiResolver.Effective(configuredTarget, ctx.Tools == null ? null : ctx.Tools.Sdk);
            if (effective < 0 || effective >= minimum) yield break;

            var evidence = new List<string>
            {
                configuredTarget > 0
                    ? Messages.Get("PRJ001.evidence.target", configuredTarget)
                    : Messages.Get("PRJ001.evidence.auto", effective),
                Messages.Get("PRJ001.evidence.minimum", minimum),
            };
            if (!string.IsNullOrEmpty(ctx.Play.minTargetSdkDeadline))
                evidence.Add(Messages.Get("PRJ001.evidence.deadline", ctx.Play.minTargetSdkDeadline));

            yield return new Finding("PRJ001", Category.Project, Severity.Error,
                Messages.Get("PRJ001.title", effective, minimum), Messages.Get("PRJ001.detail", minimum),
                evidence, FixIds.TargetApi, Messages.Get("PRJ001.fix", minimum));
        }
    }
}
