using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AndroidDoctor
{
    public static class RuleRunner
    {
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
        public static List<Finding> RunAll(IList<IRule> rules, RuleContext ctx, Action<float, string> progress = null)
        {
            var all = new List<Finding>();
            for (int i = 0; i < rules.Count; i++)
            {
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
