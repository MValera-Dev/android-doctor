using System.Collections.Generic;

namespace AndroidDoctor.Gradle
{
    /// <summary>
    /// GRD100: if the last Android build failed in Gradle, the probable cause is part of the normal checks, with its buttons.
    /// The failure comes from this session's console capture first, then from the Editor log. A build that worked
    /// afterwards clears it.
    /// </summary>
    public sealed class LastBuildFailureRule : IRule
    {
        public string Id => "GRD100";
        public Category Category => Category.Gradle;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            return Evaluate(GradleErrorCapture.Get() ?? LastBuildAnalyzer.Analyze(), null);
        }

        internal static IEnumerable<Finding> Evaluate(GradleError error, ProjectIndex project)
        {
            if (error == null) yield break;
            foreach (var finding in GradleTranslator.Translate(error, null, project)) yield return finding;
        }
    }
}
