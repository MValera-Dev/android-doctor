using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using AndroidDoctor.Environment;
using AndroidDoctor.Project;
using AndroidDoctor.UI;

namespace AndroidDoctor.Tests
{
    public class FixesTests
    {
        AndroidSdkVersions _targetSdk, _minSdk;
        AndroidArchitecture _architectures;
        ScriptingImplementation _backend;
        UnityEngine.Object _selection;

        [SetUp] public void SetUp()
        {
            _targetSdk = PlayerSettings.Android.targetSdkVersion;
            _minSdk = PlayerSettings.Android.minSdkVersion;
            _architectures = PlayerSettings.Android.targetArchitectures;
            _backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android);
            _selection = Selection.activeObject;
        }

        [TearDown] public void TearDown()
        {
            PlayerSettings.Android.targetSdkVersion = _targetSdk;
            PlayerSettings.Android.minSdkVersion = _minSdk;
            PlayerSettings.Android.targetArchitectures = _architectures;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, _backend);
            Selection.activeObject = _selection;
        }

        static Finding FindingWithFix(string fixId, params string[] evidence)
        {
            return new Finding("TST000", Category.Project, Severity.Error, "title", "detail", evidence, fixId, "label");
        }

        // --- registry ---

        [Test] public void Registry_EveryFixIdHasExactlyOneFix()
        {
            foreach (string id in FixIds.All)
            {
                var fix = FixRegistry.Get(id);
                Assert.IsNotNull(fix, "no fix registered for " + id);
                Assert.AreEqual(id, fix.Id);
            }
            var ids = FixRegistry.DiscoverFixes().Select(f => f.Id).ToList();
            CollectionAssert.AllItemsAreUnique(ids);
            CollectionAssert.AreEquivalent(FixIds.All, ids);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("FIX_DOES_NOT_EXIST")]
        public void Registry_UnknownId_ReturnsNull(string id)
        {
            Assert.IsNull(FixRegistry.Get(id));
        }

        [Test] public void EveryFixIdUsedByARuleIsRegistered()
        {
            // Rules that can offer a fix, run against contexts built to trigger them.
            var ctx = TestContexts.Create();
            ctx.Play = PlayRequirements.Parse(@"{ ""minTargetSdk"": 36, ""require64Bit"": true }");
            var findings = TargetApiRule.Evaluate(ctx, 30)
                .Concat(MinApiRule.Evaluate(ctx, 21))
                .Concat(Arm64Il2CppRule.Evaluate(ctx, false, false, "ARMv7", "Mono2x"));
            foreach (var f in findings)
                Assert.IsNotNull(FixRegistry.Get(f.FixId), f.RuleId + " offers " + f.FixId);
        }

        // --- flow ---

        [Test] public void Flow_Declined_ChangesNothing_AndShowsBeforeAfter()
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            string shown = null;
            string message;

            bool applied = FixFlow.Run(FindingWithFix(FixIds.TargetApi), text => { shown = text; return false; }, out message);

            Assert.IsFalse(applied);
            Assert.IsNull(message);
            Assert.AreEqual(AndroidSdkVersions.AndroidApiLevelAuto, PlayerSettings.Android.targetSdkVersion);
            StringAssert.Contains("Auto", shown);
            StringAssert.Contains("->", shown);
            StringAssert.Contains("36", shown);
        }

        [Test] public void Flow_Accepted_AppliesTheFix()
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            string message;

            bool applied = FixFlow.Run(FindingWithFix(FixIds.TargetApi), text => true, out message);

            Assert.IsTrue(applied, message);
            Assert.AreEqual(AndroidSdkVersions.AndroidApiLevel36, PlayerSettings.Android.targetSdkVersion);
            StringAssert.Contains("36", message);
        }

        [Test] public void Flow_UnknownFix_AppliesNothingAndDoesNotAsk()
        {
            bool asked = false;
            string message;
            bool applied = FixFlow.Run(FindingWithFix("FIX_DOES_NOT_EXIST"), text => { asked = true; return true; }, out message);
            Assert.IsFalse(applied);
            Assert.IsFalse(asked);
        }

        // --- the fixes ---

        [Test] public void MinApiFix_SetsTheRequiredMinimum()
        {
            var fix = new SetMinApiFix();
            string message;
            Assert.IsTrue(fix.Apply(FindingWithFix(FixIds.MinApi), out message), message);
            Assert.AreEqual(AndroidSdkVersions.AndroidApiLevel26, PlayerSettings.Android.minSdkVersion);
            StringAssert.Contains("26", fix.Describe(FindingWithFix(FixIds.MinApi)));
        }

        [Test] public void Arm64Fix_EnablesArm64AndIl2Cpp()
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            var fix = new EnableArm64Fix();

            string description = fix.Describe(FindingWithFix(FixIds.Arm64));
            StringAssert.Contains("ARMv7", description);
            StringAssert.Contains("ARM64", description);
            StringAssert.Contains("IL2CPP", description);

            string message;
            Assert.IsTrue(fix.Apply(FindingWithFix(FixIds.Arm64), out message), message);
            Assert.AreNotEqual(0, PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64);
            Assert.AreEqual(ScriptingImplementation.IL2CPP, PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android));
        }

        [Test] public void SelectAssetFix_SelectsTheAsset_AndFailsCleanlyForAMissingOne()
        {
            var fix = new SelectAssetFix();
            string message;

            var existing = FindingWithFix(FixIds.SelectAsset, "Packages/com.mvalera_dev.androiddoctor/package.json");
            Assert.IsTrue(fix.Apply(existing, out message), message);
            Assert.IsNotNull(Selection.activeObject);

            var missing = FindingWithFix(FixIds.SelectAsset, "Assets/does-not-exist.aar");
            Assert.IsFalse(fix.Apply(missing, out message));
            Assert.IsNotEmpty(message);

            Assert.IsFalse(fix.Apply(FindingWithFix(FixIds.SelectAsset), out message)); // no evidence at all
        }

        [Test] public void OpenExternalToolsFix_DescribesItselfAsChangingNothing()
        {
            var fix = new OpenExternalToolsFix();
            Assert.AreEqual(FixIds.OpenExternalTools, fix.Id);
            Assert.IsNotEmpty(fix.Describe(FindingWithFix(FixIds.OpenExternalTools)));
        }
    }
}
