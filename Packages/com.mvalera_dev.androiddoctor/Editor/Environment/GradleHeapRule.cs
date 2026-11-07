using System.Collections.Generic;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV010: the heap Unity gives Gradle is small enough to run out of memory on a normal project.</summary>
    public sealed class GradleHeapRule : IRule
    {
        public string Id => "ENV010";
        public Category Category => Category.Environment;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ExternalToolsSettings.HeapMegabytes);

        internal static IEnumerable<Finding> Evaluate(int heapMegabytes)
        {
            if (heapMegabytes < 0 || heapMegabytes >= RaiseHeapFix.MinimumHealthy) yield break; // unknown, or enough
            yield return new Finding("ENV010", Category.Environment, Severity.Warning,
                Messages.Get("ENV010.title", heapMegabytes), Messages.Get("ENV010.detail"),
                new[] { Messages.Get("ENV010.evidence", heapMegabytes) },
                actions: new[] { new FixAction(FixIds.Heap, Messages.Get("ENV010.fix"), null, true) });
        }
    }
}
