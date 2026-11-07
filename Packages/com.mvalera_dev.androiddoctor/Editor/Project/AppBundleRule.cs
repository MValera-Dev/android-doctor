using System.Collections.Generic;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>PRJ004 (informational): Google Play expects an Android App Bundle for new apps.</summary>
    public sealed class AppBundleRule : IRule
    {
        public string Id => "PRJ004";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(EditorUserBuildSettings.buildAppBundle);

        internal static IEnumerable<Finding> Evaluate(bool buildAppBundle)
        {
            if (buildAppBundle) yield break;
            yield return new Finding("PRJ004", Category.Project, Severity.Info,
                Messages.Get("PRJ004.title"), Messages.Get("PRJ004.detail"),
                new[] { Messages.Get("PRJ004.evidence") },
                actions: new[] { new FixAction(FixIds.AppBundle, Messages.Get("PRJ004.fix")) });
        }
    }
}
