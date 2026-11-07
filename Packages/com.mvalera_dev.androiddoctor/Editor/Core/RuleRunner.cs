using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AndroidDoctor
{
    public static class RuleRunner
    {
        public const string TimeoutRuleId = "RUN001";

        /// <summary>Finds every concrete IRule with a public parameterless constructor, ordered by id.</summary>
        public static List<IRule> DiscoverRules()
        {
            var rules = new List<IRule>();
            foreach (var t in TypeCache.GetTypesDerivedFrom<IRule>())
            {
                if (t.IsAbstract || t.IsInterface || t.GetConstructor(Type.EmptyTypes) == null) continue;
                rules.Add((IRule)Activator.CreateInstance(t));
            }
            return rules.OrderBy(r => r.Id, StringComparer.Ordinal).ToList();
        }

        /// <summary>
        /// Runs every rule. A rule that throws never aborts the run: it becomes an Info finding.
        /// Results are ordered by severity (errors first), then category, then rule id.
        /// </summary>
        /// <param name="timeBudget">
        /// When set, no new rule starts once this much time has passed; the run ends with an Info finding that says
        /// how many rules were skipped, so a slow project never freezes the window.
        /// </param>
        public static List<Finding> RunAll(IList<IRule> rules, RuleContext ctx, Action<float, string> progress = null,
            TimeSpan? timeBudget = null)
        {
            var all = new List<Finding>();
            var clock = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < rules.Count; i++)
            {
                if (timeBudget.HasValue && clock.Elapsed > timeBudget.Value)
                {
                    all.Add(new Finding(TimeoutRuleId, Category.General, Severity.Info,
                        Messages.Get("runner.timeout.title", (int)timeBudget.Value.TotalSeconds),
                        Messages.Get("runner.timeout.detail", rules.Count - i)));
                    break;
                }
                var rule = rules[i];
                progress?.Invoke((float)i / rules.Count, rule.Id);
                try
                {
                    var found = rule.Run(ctx);
                    if (found != null) all.AddRange(found);
                }
                catch (Exception e)
                {
                    all.Add(new Finding(rule.Id, rule.Category, Severity.Info,
                        Messages.Get("runner.ruleFailed.title", rule.Id), e.GetType().Name + ": " + e.Message));
                }
            }
            return all.OrderByDescending(f => f.Severity).ThenBy(f => f.Category).ThenBy(f => f.RuleId, StringComparer.Ordinal).ToList();
        }
    }
}
