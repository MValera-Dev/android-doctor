using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV004: the SDK platform for the target API level is installed.</summary>
    public sealed class SdkPlatformRule : IRule
    {
        public string Id => "ENV004";
        public Category Category => Category.Environment;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            // AndroidSdkVersions.AndroidApiLevelAuto is 0; any other value is the API level itself.
            return Evaluate(ctx, (int)PlayerSettings.Android.targetSdkVersion);
        }

        /// <param name="targetApi">The configured target API level; 0 or less means Auto.</param>
        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, int targetApi)
        {
            var sdk = ctx.Tools == null ? null : ctx.Tools.Sdk;
            // A missing SDK folder is ENV003's finding, not this rule's.
            if (sdk == null || string.IsNullOrEmpty(sdk.Path) || !sdk.Exists) yield break;

            var installed = AndroidSdkLayout.InstalledApis(sdk.Path);
            string installedText = installed.Count == 0
                ? Messages.Get("ENV004.evidence.none")
                : string.Join(", ", installed.Select(a => "android-" + a));
            var evidence = new[]
            {
                Messages.Get("ENV004.evidence.path", sdk.Path),
                Messages.Get("ENV004.evidence.installed", installedText),
            };

            if (targetApi <= 0)
            {
                // Auto uses the highest installed platform, so the only failure is having none.
                if (installed.Count > 0) yield break;
                int suggested = ctx.Play == null ? -1 : ctx.Play.minTargetSdk;
                yield return new Finding("ENV004", Category.Environment, Severity.Error,
                    Messages.Get("ENV004.none.title"), Messages.Get("ENV004.none.detail"), evidence,
                    command: suggested > 0 ? SdkManagerCommand(suggested) : null);
                yield break;
            }

            if (installed.Contains(targetApi)) yield break;
            yield return new Finding("ENV004", Category.Environment, Severity.Error,
                Messages.Get("ENV004.missing.title", targetApi), Messages.Get("ENV004.missing.detail", targetApi), evidence,
                command: SdkManagerCommand(targetApi));
        }

        static string SdkManagerCommand(int api) => "sdkmanager \"platforms;android-" + api + "\"";
    }
}
