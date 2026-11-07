using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Gradle;

namespace AndroidDoctor.Tests
{
    /// <summary>The buttons a real Gradle failure gets, decided from the error and from a described project.</summary>
    public class GradleFixesTests
    {
        sealed class FakeProject : ProjectIndex
        {
            public List<string> Libraries = new List<string>();
            public List<string> Natives = new List<string>();
            public List<string> Manifests = new List<string>();
            public List<string> Templates = new List<string>();
            public HashSet<string> CustomTools = new HashSet<string>();
            public bool Resolver;
            public List<string> LastCandidates;

            public override List<string> FindLibraries(IList<string> candidates) { LastCandidates = candidates.ToList(); return Libraries; }
            public override List<string> FindNativeLibraries(string fileName) => Natives;
            public override List<string> ManifestsMissingExported() => Manifests;
            public override List<string> CustomTemplates() => Templates;
            public override bool ToolIsCustom(string kind) => CustomTools.Contains(kind);
            public override bool ResolverAvailable => Resolver;
        }

        static Finding Translate(string fixture, FakeProject project, string envVar = null)
        {
            var error = GradleLogParser.Extract(GradleFixtures.Read(fixture));
            Assert.IsNotNull(error, fixture);
            return GradleTranslator.Translate(error, n => n == envVar ? "C:\\x" : null, project).First();
        }

        [Test] public void Grd009_OutOfMemory_RaisesTheHeap_InFixAll()
        {
            var f = Translate("GRD009_1", new FakeProject());
            Assert.AreEqual(FixIds.Heap, f.Actions.Single().FixId);
            Assert.IsTrue(f.Actions[0].AllInOne);
        }

        [Test] public void Grd011_ALibraryNeedsAHigherMinApi_SetsExactlyThatLevel()
        {
            var f = Translate("GRD011_1", new FakeProject());
            Assert.AreEqual(FixIds.MinApi, f.Actions.Single().FixId);
            Assert.AreEqual("30", f.Actions[0].Arg, "the log says the library declares 30");
            Assert.IsTrue(f.Actions[0].AllInOne);
        }

        [Test] public void Grd015_SdkHome_IsIgnoredForTheSession()
        {
            var f = Translate("GRD015_1", new FakeProject(), "ANDROID_SDK_HOME");
            Assert.AreEqual(FixIds.ClearEnvironmentVariable, f.Actions.Single().FixId);
            Assert.AreEqual("ANDROID_SDK_HOME", f.Actions[0].Arg);
        }

        [Test] public void Grd006_DuplicateClass_OffersToTurnOffEachLibraryTheProjectHas()
        {
            var project = new FakeProject { Libraries = { "Assets/Plugins/Android/dupa-1.0.jar", "Assets/Plugins/Android/dupb-1.0.jar" } };
            var f = Translate("GRD006_1", project);

            CollectionAssert.Contains(project.LastCandidates, "dupa-1.0.jar", "the log names its libraries by file");
            Assert.AreEqual(2, f.Actions.Count);
            Assert.IsTrue(f.Actions.All(a => a.FixId == FixIds.DisablePlugin));
            Assert.IsFalse(f.Actions.Any(a => a.AllInOne), "which library to drop is the user's choice");
            Assert.AreEqual("Assets/Plugins/Android/dupa-1.0.jar", f.Actions[0].Arg);
        }

        [Test] public void Grd006_WithTheResolverInstalled_AlsoOffersToResetIt()
        {
            var f = Translate("GRD006_1", new FakeProject { Resolver = true });
            Assert.AreEqual(FixIds.ResetResolver, f.Actions.Single().FixId);
        }

        [Test] public void Grd006_WhenNothingInTheProjectMatches_HasNoButtonRatherThanAGuess()
        {
            Assert.AreEqual(0, Translate("GRD006_1", new FakeProject()).Actions.Count);
        }

        [Test] public void CandidateNames_ReadsCoordinatesAndFileNames()
        {
            var names = ProjectIndex.CandidateNames(
                "core-1.5.0-runtime (androidx.core:core:1.5.0) and jetified-androidx.core.core-1.9.0-runtime (androidx.core:core:1.9.0) x (foo-1.0.jar)");
            CollectionAssert.Contains(names, "core-1.5.0");
            CollectionAssert.Contains(names, "androidx.core.core-1.9.0", "how EDM4U names the file");
            CollectionAssert.Contains(names, "foo-1.0.jar");
        }

        [Test] public void Grd007_Exported_OffersTheManifestsThatNeedIt()
        {
            var f = Translate("GRD007_1", new FakeProject { Manifests = { "Assets/Plugins/Android/AndroidManifest.xml" } });
            Assert.AreEqual(FixIds.Exported, f.Actions.Single().FixId);
            Assert.AreEqual("Assets/Plugins/Android/AndroidManifest.xml", f.Actions[0].Arg);
            Assert.IsTrue(f.Actions[0].AllInOne);
        }

        [Test] public void Grd008_TwoPluginsShipTheSameLibrary_OffersToTurnOffEither()
        {
            var f = Translate("GRD008_1", new FakeProject { Natives = { "Assets/A/arm64-v8a/libdup.so", "Assets/B/arm64-v8a/libdup.so" } });
            Assert.AreEqual(2, f.Actions.Count);
            Assert.IsFalse(f.Actions.Any(a => a.AllInOne));
        }

        [Test] public void Grd014_ATemplateThatDoesNotCompile_OffersToResetEveryCustomTemplate()
        {
            var f = Translate("GRD014_1", new FakeProject { Templates = { "Assets/Plugins/Android/mainTemplate.gradle", "Assets/Plugins/Android/settingsTemplate.gradle" } });
            Assert.AreEqual(FixIds.ResetTemplates, f.Actions.Single().FixId);
            StringAssert.Contains("mainTemplate.gradle|", f.Actions[0].Arg);
            Assert.IsFalse(f.Actions[0].AllInOne);
        }

        [Test] public void Grd014_WithoutCustomTemplates_HasNoButton()
        {
            Assert.AreEqual(0, Translate("GRD014_1", new FakeProject()).Actions.Count);
        }

        [Test] public void Grd005_OnlyOffersUnityNdk_WhenTheNdkIsCustom()
        {
            Assert.AreEqual(0, Translate("GRD005_1", new FakeProject()).Actions.Count);
            var f = Translate("GRD005_1", new FakeProject { CustomTools = { "ndk" } });
            Assert.AreEqual("ndk", f.Actions.Single().Arg);
        }

        [Test] public void AnUnrecognisedFailure_OffersToCleanTheGeneratedProject()
        {
            var f = Translate("UNKNOWN_1", new FakeProject());
            Assert.AreEqual(GradleTranslator.UnknownId, f.RuleId);
            Assert.AreEqual(FixIds.CleanCache, f.Actions.Single().FixId);
            Assert.IsFalse(f.Actions[0].AllInOne);
        }

        // ---- the failure of the last build, as part of the checks -----------------------------------------

        [Test] public void LastBuildRule_WithNoFailure_IsSilent()
        {
            Assert.AreEqual(0, LastBuildFailureRule.Evaluate(null, new FakeProject()).Count());
        }

        [Test] public void LastBuildRule_TurnsAFailureIntoItsFindings()
        {
            var error = GradleLogParser.Extract(GradleFixtures.Read("GRD009_1"));
            var findings = LastBuildFailureRule.Evaluate(error, new FakeProject()).ToList();
            Assert.AreEqual("GRD009", findings.Single().RuleId);
            Assert.AreEqual(Category.Gradle, findings[0].Category);
        }

        // ---- a later good build means the old failure is not the problem ----------------------------------

        [Test] public void AFailure_FollowedByASuccessfulBuild_IsNotReported()
        {
            string failed = GradleFixtures.Read("GRD009_1");
            string log = failed + "\nBuild completed with a result of 'Succeeded' in: 90 seconds\n";
            Assert.IsNull(LastBuildAnalyzer.Analyze(log, p => null));
        }

        [Test] public void AFailure_WithNoLaterSuccess_IsReported()
        {
            string log = "Build completed with a result of 'Succeeded' in: 5 seconds\n" + GradleFixtures.Read("GRD009_1");
            Assert.IsNotNull(LastBuildAnalyzer.Analyze(log, p => null), "the success came first, so the failure is the latest");
        }
    }
}
