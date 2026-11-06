using System;
using System.Collections.Generic;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV008: environment variables that can interfere with Gradle and the Android tools.</summary>
    public sealed class EnvironmentVariablesRule : IRule
    {
        const string SdkHome = "ANDROID_SDK_HOME";
        static readonly string[] Variables =
        {
            SdkHome, "ANDROID_HOME", "JAVA_HOME", "ANDROID_NDK_HOME", "GRADLE_USER_HOME",
        };

        public string Id => "ENV008";
        public Category Category => Category.Environment;

        // Inside this namespace "Environment" means AndroidDoctor.Environment, so qualify System.Environment.
        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(System.Environment.GetEnvironmentVariable);

        internal static IEnumerable<Finding> Evaluate(Func<string, string> getVariable)
        {
            foreach (string name in Variables)
            {
                string value = getVariable(name);
                if (string.IsNullOrEmpty(value)) continue;

                var evidence = new[] { Messages.Get("ENV008.evidence", name, value) };
                if (name == SdkHome)
                {
                    // Seen in real Unity build errors: it breaks applying the Android Gradle plugin.
                    yield return new Finding("ENV008", Category.Environment, Severity.Warning,
                        Messages.Get("ENV008.sdkHome.title"), Messages.Get("ENV008.sdkHome.detail"), evidence);
                }
                else
                {
                    yield return new Finding("ENV008", Category.Environment, Severity.Info,
                        Messages.Get("ENV008.other.title", name), Messages.Get("ENV008.other.detail"), evidence);
                }
            }
        }
    }
}
