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
        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(System.Environment.GetEnvironmentVariable, ctx.Tools);

        internal static IEnumerable<Finding> Evaluate(Func<string, string> getVariable) => Evaluate(getVariable, null);

        /// <param name="tools">The tools Unity uses, to recognise variables that point at the very same folder.</param>
        internal static IEnumerable<Finding> Evaluate(Func<string, string> getVariable, AndroidTools tools)
        {
            foreach (string name in Variables)
            {
                string value = getVariable(name);
                if (string.IsNullOrEmpty(value)) continue;
                // Unity itself sets JAVA_HOME to its own JDK while it builds for Android. A variable that
                // names the folder Unity already uses cannot send the build to a different tool.
                if (PointsAtUnityTool(name, value, tools)) continue;

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

        static bool PointsAtUnityTool(string variable, string value, AndroidTools tools)
        {
            if (tools == null) return false;
            ToolPath own;
            switch (variable)
            {
                case "JAVA_HOME": own = tools.Jdk; break;
                case "ANDROID_HOME": own = tools.Sdk; break;
                case "ANDROID_NDK_HOME": own = tools.Ndk; break;
                default: return false; // ANDROID_SDK_HOME and GRADLE_USER_HOME are never Unity's own
            }
            return own != null && !string.IsNullOrEmpty(own.Path) && AndroidToolsReader.SamePath(value, own.Path);
        }
    }
}
