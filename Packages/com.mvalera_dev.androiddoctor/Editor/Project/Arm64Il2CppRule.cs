using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace AndroidDoctor.Project
{
    /// <summary>PRJ003: ARM64 is enabled and the scripting backend is IL2CPP (the only backend that builds 64-bit).</summary>
    public sealed class Arm64Il2CppRule : IRule
    {
        public string Id => "PRJ003";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            var architectures = PlayerSettings.Android.targetArchitectures;
            var backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android);
            return Evaluate(ctx,
                (architectures & AndroidArchitecture.ARM64) != 0,
                backend == ScriptingImplementation.IL2CPP,
                architectures.ToString(), backend.ToString());
        }

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, bool hasArm64, bool usesIl2Cpp,
            string architecturesText, string backendText)
        {
            bool require64 = ctx.Play == null || ctx.Play.require64Bit;
            var evidence = new[]
            {
                Messages.Get("PRJ003.evidence.architectures", architecturesText),
                Messages.Get("PRJ003.evidence.backend", backendText),
            };

            if (require64 && !hasArm64)
            {
                yield return new Finding("PRJ003", Category.Project, Severity.Error,
                    Messages.Get("PRJ003.noArm64.title"), Messages.Get("PRJ003.noArm64.detail"),
                    evidence, FixIds.Arm64, Messages.Get("PRJ003.fix"));
            }
            else if (require64 && !usesIl2Cpp)
            {
                yield return new Finding("PRJ003", Category.Project, Severity.Error,
                    Messages.Get("PRJ003.mono.title"), Messages.Get("PRJ003.mono.detail"),
                    evidence, FixIds.Arm64, Messages.Get("PRJ003.fix"));
            }
        }
    }
}
