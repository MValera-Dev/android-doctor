using System;
using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Gradle;

namespace AndroidDoctor.Tests
{
    public class GradleTranslatorTests
    {
        static readonly string[] Fixtures = GradleFixtures.FailureNames();

        static string ExpectedId(string fixtureName) => fixtureName.Split('_')[0].Replace("UNKNOWN", GradleTranslator.UnknownId);

        // GRD015 only applies when ANDROID_SDK_HOME is set, so its fixture is translated with that variable present.
        static Func<string, string> EnvFor(string fixtureName)
        {
            return name => fixtureName.StartsWith("GRD015") && name == "ANDROID_SDK_HOME" ? "C:\\some\\sdk" : null;
        }

        static System.Collections.Generic.List<Finding> TranslateFixture(string name)
        {
            var error = GradleLogParser.Extract(GradleFixtures.Read(name));
            Assert.IsNotNull(error, name + " has no Gradle failure block");
            return GradleTranslator.Translate(error, EnvFor(name));
        }

        // --- every real log gives exactly its pattern ---

        [TestCaseSource(nameof(Fixtures))]
        public void RealLog_GivesExactlyItsPattern(string name)
        {
            var ids = TranslateFixture(name).Select(f => f.RuleId).ToList();
            CollectionAssert.AreEqual(new[] { ExpectedId(name) }, ids, name);
        }

        [Test] public void EveryPattern_HasAtLeastOneRealLog()
        {
            var covered = Fixtures.Select(ExpectedId).ToList();
            foreach (var pattern in GradlePatterns.All)
                CollectionAssert.Contains(covered, pattern.Id, pattern.Id + " has no fixture: a pattern without a real log is not published");
        }

        [Test] public void EveryRealLog_BelongsToAPatternOrIsTheUnknownCase()
        {
            var known = GradlePatterns.All.Select(p => p.Id).Concat(new[] { GradleTranslator.UnknownId }).ToList();
            foreach (var name in Fixtures) CollectionAssert.Contains(known, ExpectedId(name), name);
        }

        [Test] public void There_AreAtLeastFifteenPatterns()
        {
            Assert.GreaterOrEqual(GradlePatterns.All.Length, 15);
        }

        [Test] public void PatternIds_AreUnique()
        {
            CollectionAssert.AllItemsAreUnique(GradlePatterns.All.Select(p => p.Id));
        }

        [Test] public void EveryPattern_HasATitleAndADetailInEnglish()
        {
            foreach (var id in GradlePatterns.All.Select(p => p.Id).Concat(new[] { GradleTranslator.UnknownId }))
            {
                Assert.AreNotEqual(id + ".title", Messages.Get(id + ".title"), id + " has no title");
                Assert.AreNotEqual(id + ".detail", Messages.Get(id + ".detail"), id + " has no detail");
            }
        }

        // --- what the findings say ---

        [Test] public void Findings_AreGradleErrors_WithEvidenceFromTheLog()
        {
            var f = TranslateFixture("GRD006_1").Single();
            Assert.AreEqual(Category.Gradle, f.Category);
            Assert.AreEqual(Severity.Error, f.Severity);
            StringAssert.Contains("com.example.Dup", f.Evidence[0]);
        }

        [Test] public void Grd003_GivesTheSdkManagerCommand()
        {
            var f = TranslateFixture("GRD003_1").Single();
            Assert.AreEqual("sdkmanager \"platforms;android-99.0\"", f.Command);
            StringAssert.Contains("99.0", f.Title);
        }

        [Test] public void Grd004_GivesTheSdkManagerCommand()
        {
            Assert.AreEqual("sdkmanager \"build-tools;99.0.0\"", TranslateFixture("GRD004_1").Single().Command);
        }

        [Test] public void Grd005_NamesBothNdkVersions()
        {
            var f = TranslateFixture("GRD005_1").Single();
            StringAssert.Contains("99.0.0", f.Detail);
            StringAssert.Contains("27.2.12479018", f.Detail);
        }

        [Test] public void Grd007_NamesTheComponent()
        {
            var f = TranslateFixture("GRD007_1").Single();
            StringAssert.Contains("<activity>", f.Title);
            StringAssert.Contains("com.example.testlib.Foo", f.Title);
        }

        [Test] public void Grd008_NamesThePathAndTheCount()
        {
            var f = TranslateFixture("GRD008_1").Single();
            StringAssert.Contains("lib/arm64-v8a/libdup.so", f.Detail);
            StringAssert.Contains("2 ", f.Detail);
        }

        [Test] public void Grd011_NamesTheTwoApiLevelsAndTheLibrary()
        {
            var f = TranslateFixture("GRD011_1").Single();
            StringAssert.Contains("26", f.Detail);
            StringAssert.Contains("30", f.Detail);
            StringAssert.Contains("minlib-1.0.0", f.Detail);
        }

        [Test] public void Grd016_NamesTheAlias_ButNeverAPassword()
        {
            var f = TranslateFixture("GRD016_1").Single();
            StringAssert.Contains("testalias", f.Detail);
            StringAssert.DoesNotContain("WrongStorePass", f.Detail + string.Join("", f.Evidence));
        }

        [Test] public void Grd018_NamesTheMissingDependency()
        {
            StringAssert.Contains("com.example.nonexistent:lib:1.0.0", TranslateFixture("GRD018_1").Single().Title);
        }

        [Test] public void Grd019_NamesTheAttributeAndTheOtherLibrary()
        {
            var f = TranslateFixture("GRD019_1").Single();
            StringAssert.Contains("application@label", f.Title);
            StringAssert.Contains("LibA.androidlib", f.Detail);
        }

        [Test] public void Grd020_NamesTheFileTheLineAndTheProblem()
        {
            var f = TranslateFixture("GRD020_1").Single();
            StringAssert.Contains("Broken.java", f.Title);
            StringAssert.Contains("cannot find symbol", f.Detail);
            StringAssert.Contains("4", f.Detail);
        }

        // --- the environment condition of GRD015 ---

        [Test] public void Grd015_WithoutTheVariable_IsNotRecognised()
        {
            var error = GradleLogParser.Extract(GradleFixtures.Read("GRD015_1"));
            var findings = GradleTranslator.Translate(error, name => null);
            Assert.AreEqual(GradleTranslator.UnknownId, findings.Single().RuleId);
        }

        // --- unknown failures ---

        [Test] public void AnUnknownFailure_GivesOnlyGrd000_WithTheOriginalText()
        {
            var f = TranslateFixture("UNKNOWN_1").Single();
            Assert.AreEqual("GRD000", f.RuleId);
            Assert.IsTrue(f.Evidence.Any(e => e.Contains("does-not-exist.gradle")));
            Assert.LessOrEqual(f.Evidence.Count, 3);
        }

        [Test] public void NullError_GivesNoFindings()
        {
            Assert.IsEmpty(GradleTranslator.Translate(null));
        }

        [Test] public void ACleanBuild_GivesNothing()
        {
            Assert.IsNull(GradleLogParser.Extract(GradleFixtures.Read("CLEAN_1")));
            Assert.IsEmpty(GradleTranslator.Translate(GradleLogParser.Extract(GradleFixtures.Read("CLEAN_1"))));
        }

        // --- priority ---

        [Test] public void WhenSeveralPatternsMatch_TheOnesWithLowerPriorityNumbersComeFirst()
        {
            var error = new GradleError
            {
                Block = "Failed to apply plugin 'com.android.internal.application'.\nDuplicate class a.B found in modules x and y\nCould not HEAD 'https://example.com/a'",
                WhatWentWrong = "",
            };
            var ids = GradleTranslator.Translate(error, name => name == "ANDROID_SDK_HOME" ? "x" : null).Select(f => f.RuleId).ToList();
            CollectionAssert.AreEqual(new[] { "GRD015", "GRD006", "GRD010" }, ids);
        }

        [Test] public void ATextAskingForMoreValuesThanWereGiven_DoesNotBreakTheDiagnosis()
        {
            // PRJ001.title wants two values; giving one must not throw.
            string text = null;
            Assert.DoesNotThrow(() => text = Messages.Get("PRJ001.title", 34));
            Assert.IsNotEmpty(text);
        }
    }
}
