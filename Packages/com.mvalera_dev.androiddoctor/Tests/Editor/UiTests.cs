using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using AndroidDoctor.Gradle;
using AndroidDoctor.UI;

namespace AndroidDoctor.Tests
{
    public class FindingCardTests
    {
        static Finding F(Severity severity = Severity.Error, string[] evidence = null, string fixId = null,
            string command = null, string docs = null)
        {
            return new Finding("ENV004", Category.Environment, severity, "A title", "A detail sentence.", evidence,
                fixId, "Do the fix", command, docs);
        }

        // An element outside a panel never receives events, so the card is hosted in a throw-away editor window
        // while the button is pressed with the same event a keyboard Submit would send.
        static void Press(Button button)
        {
            VisualElement top = button;
            while (top.hierarchy.parent != null) top = top.hierarchy.parent;

            var window = ScriptableObject.CreateInstance<EditorWindow>();
            try
            {
                window.Show();
                window.rootVisualElement.Add(top);
                using (var e = NavigationSubmitEvent.GetPooled())
                {
                    e.target = button;
                    button.SendEvent(e);
                }
                top.RemoveFromHierarchy();
            }
            finally { window.Close(); }
        }

        [Test] public void ShowsTheRuleIdThenTheTitle_AndTheDetail()
        {
            var card = new FindingCard(F(), null);
            Assert.AreEqual("ENV004", card.IdLabel.text);
            Assert.AreEqual("A title", card.TitleLabel.text);
            Assert.AreEqual(FontStyle.Bold, card.TitleLabel.style.unityFontStyleAndWeight.value);
            Assert.AreEqual("A detail sentence.", card.DetailLabel.text);
            Assert.AreEqual(WhiteSpace.Normal, card.DetailLabel.style.whiteSpace.value);
        }

        [TestCase(Severity.Error, 0xD9, 0x53, 0x4F)]
        [TestCase(Severity.Warning, 0xE0, 0xA8, 0x00)]
        [TestCase(Severity.Info, 0x5B, 0x8D, 0xEF)]
        public void TheLeftBorderIs4pxInTheColourOfTheSeverity(Severity severity, int r, int g, int b)
        {
            var card = new FindingCard(F(severity), null);
            Assert.AreEqual(4f, card.style.borderLeftWidth.value);
            Assert.AreEqual((Color)new Color32((byte)r, (byte)g, (byte)b, 255), card.style.borderLeftColor.value);
        }

        [Test] public void Evidence_IsInAFoldoutThatStartsClosed_WithSelectableReadOnlyText()
        {
            var card = new FindingCard(F(evidence: new[] { "first line", "second line" }), null);
            Assert.IsNotNull(card.EvidenceFoldout);
            Assert.IsFalse(card.EvidenceFoldout.value);
            var field = card.EvidenceFoldout.Q<TextField>();
            Assert.IsTrue(field.multiline);
            Assert.IsTrue(field.isReadOnly);
            Assert.AreEqual("first line\nsecond line", field.value);
        }

        [Test] public void NoEvidence_NoFoldout()
        {
            Assert.IsNull(new FindingCard(F(), null).EvidenceFoldout);
        }

        [Test] public void NoFixNoCommandNoDocs_NoButtons()
        {
            var card = new FindingCard(F(), null);
            Assert.IsNull(card.FixButton);
            Assert.IsNull(card.CopyCommandButton);
            Assert.IsNull(card.DocsButton);
            Assert.AreEqual(0, card.Query<Button>().ToList().Count);
        }

        [Test] public void AFixThatExists_GetsAButtonWithItsLabel_AndPressingItCallsBack()
        {
            Finding pressed = null;
            var finding = F(fixId: FixIds.TargetApi);
            var card = new FindingCard(finding, f => pressed = f);

            Assert.IsNotNull(card.FixButton);
            Assert.AreEqual("Do the fix", card.FixButton.text);
            Press(card.FixButton);
            Assert.AreSame(finding, pressed);
        }

        [Test] public void AFixIdThatNobodyImplements_GetsNoButton()
        {
            Assert.IsNull(new FindingCard(F(fixId: "FIX_NOBODY_HAS_THIS"), null).FixButton);
        }

        [Test] public void ACommand_GetsACopyButton_ThatCopiesIt()
        {
            string original = EditorGUIUtility.systemCopyBuffer;
            try
            {
                string notified = null;
                var card = new FindingCard(F(command: "sdkmanager \"platforms;android-36\""), null, m => notified = m);

                Assert.IsNotNull(card.CopyCommandButton);
                Press(card.CopyCommandButton);

                Assert.AreEqual("sdkmanager \"platforms;android-36\"", EditorGUIUtility.systemCopyBuffer);
                Assert.IsNotNull(notified);
            }
            finally { EditorGUIUtility.systemCopyBuffer = original; }
        }

        [Test] public void ADocsUrl_GetsADocsButton()
        {
            Assert.IsNotNull(new FindingCard(F(docs: "https://example.com/docs"), null).DocsButton);
        }
    }

    public class ChecksPanelTests
    {
        static Finding F(string id, Category category, Severity severity)
        {
            return new Finding(id, category, severity, "Title " + id, "Detail " + id);
        }

        [Test] public void StartsIdle_WithoutRunningAnything()
        {
            var panel = new ChecksPanel(null, null);
            Assert.AreEqual(Messages.Get("ui.idle"), panel.SummaryText);
            Assert.AreEqual(0, panel.Results.Query<FindingCard>().ToList().Count);
        }

        [Test] public void Findings_AreGroupedByCategoryInEnumOrder_WithCounts()
        {
            var panel = new ChecksPanel(null, null);
            panel.Show(new List<Finding>
            {
                F("NAT001", Category.Native, Severity.Error),
                F("ENV001", Category.Environment, Severity.Error),
                F("ENV002", Category.Environment, Severity.Warning),
                F("PRJ001", Category.Project, Severity.Info),
            });

            var foldouts = panel.Results.Query<Foldout>().Where(f => f.text.Contains("(")).ToList();
            CollectionAssert.AreEqual(new[]
            {
                Messages.Get("ui.category.Environment") + " (2)",
                Messages.Get("ui.category.Project") + " (1)",
                Messages.Get("ui.category.Native") + " (1)",
            }, foldouts.Select(f => f.text).ToArray());
            Assert.AreEqual(4, panel.Results.Query<FindingCard>().ToList().Count);
        }

        [Test] public void EmptyCategories_AreNotShown()
        {
            var panel = new ChecksPanel(null, null);
            panel.Show(new List<Finding> { F("ENV001", Category.Environment, Severity.Error) });
            Assert.AreEqual(1, panel.Results.Query<Foldout>().Where(f => f.text.Contains("(")).ToList().Count);
        }

        [Test] public void Summary_CountsEachSeverity_WithSingularAndPlural()
        {
            var one = ChecksPanel.Summarize(new List<Finding> { F("A", Category.Project, Severity.Error) });
            Assert.AreEqual(Messages.Get("ui.errors.one", 1), one);

            var mixed = ChecksPanel.Summarize(new List<Finding>
            {
                F("A", Category.Project, Severity.Error), F("B", Category.Project, Severity.Error),
                F("C", Category.Project, Severity.Warning), F("D", Category.Project, Severity.Info),
            });
            Assert.AreEqual(Messages.Get("ui.errors.many", 2) + ", " + Messages.Get("ui.warnings.one", 1) + ", " + Messages.Get("ui.infos", 1), mixed);
        }

        [Test] public void NoFindings_SaysEverythingLooksFine()
        {
            var panel = new ChecksPanel(null, null);
            panel.Show(new List<Finding>());
            Assert.AreEqual(Messages.Get("ui.noFindings"), panel.SummaryText);
        }

        [Test] public void AnInternalError_ShowsOneMessageAndACopyButton()
        {
            var panel = new ChecksPanel(null, null);
            panel.ShowInternalError(new System.InvalidOperationException("boom"));
            Assert.AreEqual(Messages.Get("ui.error.title"), panel.SummaryText);
            Assert.AreEqual(1, panel.Results.Query<Button>().ToList().Count);
            Assert.IsTrue(panel.Results.Query<Label>().ToList().Any(l => l.text.Contains("boom")));
        }

        [Test] public void ShowingAgain_ReplacesThePreviousResults()
        {
            var panel = new ChecksPanel(null, null);
            panel.Show(new List<Finding> { F("A", Category.Project, Severity.Error), F("B", Category.Project, Severity.Error) });
            panel.Show(new List<Finding> { F("C", Category.Project, Severity.Error) });
            Assert.AreEqual(1, panel.Results.Query<FindingCard>().ToList().Count);
        }
    }

    public class GradleLogPanelTests
    {
        string _captured;

        [SetUp] public void SetUp() { _captured = SessionState.GetString(GradleErrorCapture.SessionKey, ""); GradleErrorCapture.Clear(); }
        [TearDown] public void TearDown() { SessionState.SetString(GradleErrorCapture.SessionKey, _captured); }

        [Test] public void APastedLog_ShowsTheProbableCause_TheOriginalBlockAndTheForumButton()
        {
            var panel = new GradleLogPanel(null);
            panel.Input.value = GradleFixtures.Read("GRD006_1");

            panel.AnalyzePasted();

            var cards = panel.Results.Query<FindingCard>().ToList();
            Assert.AreEqual(1, cards.Count);
            Assert.AreEqual("GRD006", cards[0].Finding.RuleId);
            Assert.IsNull(cards[0].FixButton, "Gradle problems are fixed by hand: no fix buttons");

            var original = panel.Results.Query<Foldout>().Where(f => f.text == Messages.Get("ui.gradle.original")).First();
            Assert.IsFalse(original.value, "the original block is folded by default");
            StringAssert.Contains("Duplicate class", original.Q<TextField>().value);

            Assert.IsTrue(panel.Results.Query<Button>().ToList().Any(b => b.text == Messages.Get("ui.gradle.copyForum")));
            Assert.AreEqual("", panel.MessageText);
        }

        [Test] public void TextWithoutAGradleFailure_SaysSo()
        {
            var panel = new GradleLogPanel(null);
            panel.Input.value = "nothing useful in here";
            panel.AnalyzePasted();
            Assert.AreEqual(Messages.Get("ui.gradle.notFound"), panel.MessageText);
            Assert.AreEqual(0, panel.Results.childCount);
        }

        [Test] public void AnUnrecognisedFailure_StillShowsTheOriginalBlock()
        {
            var panel = new GradleLogPanel(null);
            panel.Input.value = GradleFixtures.Read("UNKNOWN_1");
            panel.AnalyzePasted();
            Assert.AreEqual("GRD000", panel.Results.Query<FindingCard>().First().Finding.RuleId);
            Assert.IsNotNull(panel.Results.Query<Foldout>().Where(f => f.text == Messages.Get("ui.gradle.original")).First());
        }

        [Test] public void UseCaptured_WithNothingCaptured_SaysSo()
        {
            var panel = new GradleLogPanel(null);
            panel.UseCaptured();
            Assert.AreEqual(Messages.Get("ui.gradle.noCaptured"), panel.MessageText);
        }

        [Test] public void UseCaptured_ShowsTheErrorCapturedInThisSession()
        {
            SessionState.SetString(GradleErrorCapture.SessionKey, GradleFixtures.Read("GRD014_1"));
            var panel = new GradleLogPanel(null);
            panel.UseCaptured();
            Assert.AreEqual("GRD014", panel.Results.Query<FindingCard>().First().Finding.RuleId);
        }

        [Test] public void AnalysingAgain_ReplacesTheEarlierResult()
        {
            var panel = new GradleLogPanel(null);
            panel.Show(GradleLogParser.Extract(GradleFixtures.Read("GRD006_1")), "ui.gradle.notFound");
            panel.Show(GradleLogParser.Extract(GradleFixtures.Read("GRD014_1")), "ui.gradle.notFound");
            Assert.AreEqual(1, panel.Results.Query<FindingCard>().ToList().Count);
            Assert.AreEqual("GRD014", panel.Results.Query<FindingCard>().First().Finding.RuleId);
        }
    }

    public class RunBudgetTests
    {
        sealed class SlowRule : IRule
        {
            readonly string _id; readonly int _ms;
            SlowRule(string id, int ms) { _id = id; _ms = ms; }
            public static IRule Create(string id, int ms) => new SlowRule(id, ms);
            public string Id => _id;
            public Category Category => Category.Project;
            public IEnumerable<Finding> Run(RuleContext ctx)
            {
                System.Threading.Thread.Sleep(_ms);
                return new[] { new Finding(_id, Category.Project, Severity.Warning, "slow " + _id, "d") };
            }
        }

        [Test] public void WhenTheBudgetRunsOut_TheRemainingRulesAreSkipped_AndTheRunSaysSo()
        {
            var rules = new List<IRule> { SlowRule.Create("TST001", 60), SlowRule.Create("TST002", 60), SlowRule.Create("TST003", 60) };

            var findings = RuleRunner.RunAll(rules, new RuleContext(), null, System.TimeSpan.FromMilliseconds(20));

            Assert.IsTrue(findings.Any(f => f.RuleId == "TST001"), "the rule that was running finishes");
            Assert.IsFalse(findings.Any(f => f.RuleId == "TST003"));
            var stop = findings.Single(f => f.RuleId == RuleRunner.TimeoutRuleId);
            Assert.AreEqual(Severity.Info, stop.Severity);
            Assert.AreEqual(Category.General, stop.Category);
        }

        sealed class TextRule : IRule
        {
            TextRule() { } // private, so DiscoverRules never picks a test rule up
            public static IRule Create() => new TextRule();
            public string Id => "TST100";
            public Category Category => Category.Project;
            public IEnumerable<Finding> Run(RuleContext ctx)
            {
                return new[] { new Finding(Id, Category.Project, Severity.Warning, Messages.Get("ui.run"), Messages.Get("ui.idle")) };
            }
        }

        [Test] public void TheReport_IsInEnglish_EvenWhenTheUiIsNot()
        {
            string ui = Messages.Get("ui.run"); // Spanish on a Spanish system, English otherwise

            string report = EnglishReport.Build(new List<IRule> { TextRule.Create() }, new RuleContext());

            StringAssert.Contains("**TST100** Run checks.", report);
            StringAssert.Contains("Press Run checks.", report);
            Assert.AreEqual(ui, Messages.Get("ui.run"), "the UI language is back after the report");
        }

        [Test] public void WithoutABudget_EverythingRuns()
        {
            var rules = new List<IRule> { SlowRule.Create("TST001", 5), SlowRule.Create("TST002", 5) };
            var findings = RuleRunner.RunAll(rules, new RuleContext());
            Assert.AreEqual(2, findings.Count);
            Assert.IsFalse(findings.Any(f => f.RuleId == RuleRunner.TimeoutRuleId));
        }

        [Test] public void AGenerousBudget_DoesNotStopAnything()
        {
            var rules = new List<IRule> { SlowRule.Create("TST001", 5), SlowRule.Create("TST002", 5) };
            var findings = RuleRunner.RunAll(rules, new RuleContext(), null, System.TimeSpan.FromSeconds(30));
            Assert.AreEqual(2, findings.Count);
        }
    }
}
