using System.Collections.Generic;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV003: the SDK folder exists and has platform-tools and command-line tools.</summary>
    public sealed class SdkLayoutRule : IRule
    {
        public string Id => "ENV003";
        public Category Category => Category.Environment;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx);

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx)
        {
            var sdk = ctx.Tools == null ? null : ctx.Tools.Sdk;
            if (sdk == null || string.IsNullOrEmpty(sdk.Path)) yield break;

            var evidence = new[] { Messages.Get("ENV003.evidence.path", sdk.Path) };
            if (!sdk.Exists)
            {
                yield return new Finding("ENV003", Category.Environment, Severity.Error,
                    Messages.Get("ENV003.missing.title"), Messages.Get("ENV003.missing.detail"), evidence);
                yield break;
            }

            var missing = new List<string>();
            if (!AndroidSdkLayout.HasPlatformTools(sdk.Path)) missing.Add(Messages.Get("ENV003.component.platformTools"));
            if (!AndroidSdkLayout.HasCommandLineTools(sdk.Path)) missing.Add(Messages.Get("ENV003.component.cmdlineTools"));
            if (missing.Count == 0) yield break;

            yield return new Finding("ENV003", Category.Environment, Severity.Error,
                Messages.Get("ENV003.incomplete.title"),
                Messages.Get("ENV003.incomplete.detail", string.Join(Messages.Get("ENV003.component.separator"), missing)),
                evidence);
        }
    }
}
