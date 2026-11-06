using System.Collections.Generic;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>PRJ008: a release build without a custom keystore is signed with the debug key.</summary>
    public sealed class ReleaseSigningRule : IRule
    {
        public string Id => "PRJ008";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            return Evaluate(PlayerSettings.Android.useCustomKeystore, EditorUserBuildSettings.development);
        }

        internal static IEnumerable<Finding> Evaluate(bool useCustomKeystore, bool developmentBuild)
        {
            if (useCustomKeystore || developmentBuild) yield break;
            yield return new Finding("PRJ008", Category.Project, Severity.Warning,
                Messages.Get("PRJ008.title"), Messages.Get("PRJ008.detail"),
                new[] { Messages.Get("PRJ008.evidence.keystore"), Messages.Get("PRJ008.evidence.development") });
        }
    }
}
