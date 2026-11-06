using UnityEditor;
using UnityEditor.Build;

namespace AndroidDoctor.Project
{
    /// <summary>Turns on ARM64 and switches the scripting backend to IL2CPP.</summary>
    public sealed class EnableArm64Fix : IFix
    {
        public string Id => FixIds.Arm64;

        public string Describe(Finding f)
        {
            var before = PlayerSettings.Android.targetArchitectures;
            var after = before | AndroidArchitecture.ARM64;
            return Messages.Get("fix.arm64.describe", before, after,
                PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android), ScriptingImplementation.IL2CPP);
        }

        public bool Apply(Finding f, out string message)
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures |= AndroidArchitecture.ARM64;
            message = Messages.Get("fix.arm64.done");
            return true;
        }
    }
}
