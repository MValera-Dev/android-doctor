using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using AndroidDoctor.Environment;
using AndroidDoctor.Project;

namespace AndroidDoctor.UI
{
    /// <summary>Fills the report header from the real Editor, project and tools.</summary>
    public static class ReportEnvironmentReader
    {
        public static ReportEnvironment Capture(RuleContext ctx)
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(ReportEnvironmentReader).Assembly);
            return new ReportEnvironment
            {
                PackageVersion = package != null ? package.version : "dev",
                UnityVersion = Application.unityVersion,
                OperatingSystem = SystemInfo.operatingSystem,
                TargetApi = TargetApiResolver.Format((int)PlayerSettings.Android.targetSdkVersion),
                Backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android).ToString(),
                Architectures = PlayerSettings.Android.targetArchitectures.ToString(),
                Jdk = Describe(ctx.Tools == null ? null : ctx.Tools.Jdk, JdkRelease.ReadVersion),
                Ndk = Describe(ctx.Tools == null ? null : ctx.Tools.Ndk, NdkProperties.ReadRevision),
                Gradle = Describe(ctx.Tools == null ? null : ctx.Tools.Gradle, GradleVersion.FromLauncherJar),
            };
        }

        // "17.0.18 (embedded)", "21.0.2 (custom)", or "unknown" when the path or the version cannot be read.
        static string Describe(ToolPath tool, Func<string, string> readVersion)
        {
            if (tool == null || string.IsNullOrEmpty(tool.Path)) return "unknown";
            string version = readVersion(tool.Path);
            if (version == null) return "unknown";
            string source = tool.Source == ToolSource.Embedded ? "embedded" : tool.Source == ToolSource.Custom ? "custom" : "unknown source";
            return version + " (" + source + ")";
        }
    }
}
