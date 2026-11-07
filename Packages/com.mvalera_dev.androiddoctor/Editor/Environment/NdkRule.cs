using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV006: with IL2CPP, the NDK exists and, if it is a custom one, matches the revision Unity expects.</summary>
    public sealed class NdkRule : IRule
    {
        public string Id => "ENV006";
        public Category Category => Category.Environment;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            bool il2cpp = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP;
            return Evaluate(ctx, il2cpp);
        }

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, bool usesIl2Cpp)
        {
            if (!usesIl2Cpp) yield break; // Mono builds do not need the NDK
            var ndk = ctx.Tools == null ? null : ctx.Tools.Ndk;
            if (ndk == null || string.IsNullOrEmpty(ndk.Path)) yield break;

            if (!ndk.Exists)
            {
                yield return new Finding("ENV006", Category.Environment, Severity.Error,
                    Messages.Get("ENV006.missing.title"), Messages.Get("ENV006.missing.detail"),
                    new[] { Messages.Get("ENV006.evidence.path", ndk.Path) },
                    actions: UseUnityToolsFix.ActionsFor("ndk", ndk.Source == ToolSource.Custom));
                yield break;
            }

            if (ndk.Source != ToolSource.Custom) yield break;
            var row = ctx.Compat == null ? null : ctx.Compat.Find(ctx.Unity);
            if (row == null || string.IsNullOrEmpty(row.ndk)) yield break;

            string revision = NdkProperties.ReadRevision(ndk.Path);
            if (revision == null || revision == row.ndk) yield break;

            yield return new Finding("ENV006", Category.Environment, Severity.Warning,
                Messages.Get("ENV006.version.title"),
                Messages.Get("ENV006.version.detail", ctx.Unity.Key, row.ndk, revision),
                new[] { Messages.Get("ENV006.evidence.path", ndk.Path), Messages.Get("ENV006.evidence.version", revision) },
                actions: UseUnityToolsFix.ActionsFor("ndk", true));
        }
    }
}
