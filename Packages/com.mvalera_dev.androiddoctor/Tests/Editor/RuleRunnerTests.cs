using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AndroidDoctor.Tests
{
    public class RuleRunnerTests
    {
        // Test rules use private constructors on purpose: RuleRunner.DiscoverRules() only picks up
        // types with a public parameterless constructor, so these never leak into the real window.
        sealed class ThrowingRule : IRule
        {
            ThrowingRule() { }
            public static IRule Create() => new ThrowingRule();
            public string Id => "TST001";
            public Category Category => Category.Project;
            public IEnumerable<Finding> Run(RuleContext ctx) { throw new InvalidOperationException("boom"); }
        }

        sealed class FixedRule : IRule
        {
            readonly string _id; readonly Category _category; readonly Severity _severity;
            FixedRule(string id, Category category, Severity severity) { _id = id; _category = category; _severity = severity; }
            public static IRule Create(string id, Category category, Severity severity) => new FixedRule(id, category, severity);
            public string Id => _id;
            public Category Category => _category;
            public IEnumerable<Finding> Run(RuleContext ctx)
            {
                yield return new Finding(_id, _category, _severity, "title " + _id, "detail");
            }
        }

        [Test] public void ThrowingRule_BecomesInfoFinding_AndOthersStillRun()
        {
            var rules = new List<IRule>
            {
                FixedRule.Create("TST000", Category.Project, Severity.Warning),
                ThrowingRule.Create(),
                FixedRule.Create("TST002", Category.Project, Severity.Error),
            };

            var findings = RuleRunner.RunAll(rules, new RuleContext());

            Assert.AreEqual(3, findings.Count);
            var failed = findings.Single(f => f.RuleId == "TST001");
            Assert.AreEqual(Severity.Info, failed.Severity);
            StringAssert.Contains("InvalidOperationException", failed.Detail);
            StringAssert.Contains("boom", failed.Detail);
            Assert.IsTrue(findings.Any(f => f.RuleId == "TST000"));
            Assert.IsTrue(findings.Any(f => f.RuleId == "TST002"));
        }

        [Test] public void Results_AreOrderedBySeverityThenCategoryThenId()
        {
            var rules = new List<IRule>
            {
                FixedRule.Create("TST010", Category.Environment, Severity.Info),
                FixedRule.Create("TST011", Category.Project, Severity.Warning),
                FixedRule.Create("TST012", Category.Native, Severity.Error),
                FixedRule.Create("TST013", Category.Environment, Severity.Error),
            };

            var ids = RuleRunner.RunAll(rules, new RuleContext()).Select(f => f.RuleId).ToArray();

            CollectionAssert.AreEqual(new[] { "TST013", "TST012", "TST011", "TST010" }, ids);
        }

        [Test] public void Progress_IsReportedOncePerRule()
        {
            var rules = new List<IRule>
            {
                FixedRule.Create("TST020", Category.Project, Severity.Info),
                FixedRule.Create("TST021", Category.Project, Severity.Info),
            };
            var reported = new List<string>();

            RuleRunner.RunAll(rules, new RuleContext(), (progress, id) => reported.Add(id));

            CollectionAssert.AreEqual(new[] { "TST020", "TST021" }, reported);
        }

        [Test] public void DiscoverRules_FindsPrj009_AndExcludesTestRules()
        {
            var rules = RuleRunner.DiscoverRules();

            Assert.IsTrue(rules.Any(r => r.Id == "PRJ009"));
            Assert.IsFalse(rules.Any(r => r.Id.StartsWith("TST")));
        }

        [Test] public void DiscoveredRuleIds_AreUnique()
        {
            var ids = RuleRunner.DiscoverRules().Select(r => r.Id).ToList();
            CollectionAssert.AllItemsAreUnique(ids);
        }
    }
}
