using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>PRJ006: manifest components with an intent filter but no android:exported (an error from API 31).</summary>
    public sealed class ManifestExportedRule : IRule
    {
        const int FirstApiRequiringExported = 31;

        public string Id => "PRJ006";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            return Evaluate(ctx, (int)PlayerSettings.Android.targetSdkVersion);
        }

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, int configuredTarget)
        {
            if (string.IsNullOrEmpty(ctx.AssetsPath)) yield break;

            // Only skip when the target is known and older than API 31. If it cannot be known, assume a
            // current target (Auto means the newest platform).
            int effective = TargetApiResolver.Effective(configuredTarget, ctx.Tools == null ? null : ctx.Tools.Sdk);
            if (effective >= 0 && effective < FirstApiRequiringExported) yield break;

            foreach (string manifest in FindManifests(ctx.AssetsPath))
            {
                string text;
                try { text = File.ReadAllText(manifest); }
                catch (IOException) { continue; }
                catch (UnauthorizedAccessException) { continue; }

                string display = ProjectFileWalker.ToProjectRelative(ctx.ProjectPath, manifest);
                foreach (var component in ManifestExportedChecker.FindMissing(text))
                {
                    string name = string.IsNullOrEmpty(component.Name) ? Messages.Get("PRJ006.unnamed") : component.Name;
                    yield return new Finding("PRJ006", Category.Project, Severity.Error,
                        Messages.Get("PRJ006.title", component.Tag, name), Messages.Get("PRJ006.detail"),
                        new[] { Messages.Get("PRJ006.evidence", display, component.Line, component.Tag, name) },
                        actions: new[] { new FixAction(FixIds.Exported, Messages.Get("PRJ006.fix"), display, true) });
                }
            }
        }

        /// <summary>The main manifest in Assets/Plugins/Android and any manifest inside a *.androidlib folder.</summary>
        internal static List<string> FindManifests(string assetsPath)
        {
            return ProjectFileWalker.FindFiles(assetsPath, file =>
            {
                if (!string.Equals(Path.GetFileName(file), "AndroidManifest.xml", StringComparison.OrdinalIgnoreCase)) return false;
                string normalized = file.Replace('\\', '/');
                return normalized.EndsWith("/Plugins/Android/AndroidManifest.xml", StringComparison.OrdinalIgnoreCase)
                    || IsInsideAndroidLib(normalized);
            });
        }

        static bool IsInsideAndroidLib(string normalizedPath)
        {
            foreach (string part in normalizedPath.Split('/'))
                if (part.EndsWith(".androidlib", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
