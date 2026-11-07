using System.Collections.Generic;

namespace AndroidDoctor.UI
{
    /// <summary>
    /// The report that Copy report puts on the clipboard. It is pasted in forums and issues, so it is always written
    /// in English: the checks run again inside an English scope, because the findings carry their text already translated.
    /// </summary>
    internal static class EnglishReport
    {
        public static readonly System.TimeSpan DefaultBudget = System.TimeSpan.FromSeconds(30);

        public static string BuildCurrent()
        {
            using (Messages.English())
            {
                var ctx = RuleContext.Create();
                return Build(RuleRunner.DiscoverRules(), ctx);
            }
        }

        internal static string Build(IList<IRule> rules, RuleContext ctx)
        {
            using (Messages.English())
            {
                var findings = RuleRunner.RunAll(rules, ctx, null, DefaultBudget);
                return ReportBuilder.Build(ReportEnvironmentReader.Capture(ctx), findings);
            }
        }
    }
}
