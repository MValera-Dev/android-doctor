using System.Linq;
using NUnit.Framework;
using UnityEditor;
using AndroidDoctor.Environment;
using AndroidDoctor.Project;

namespace AndroidDoctor.Tests
{
    /// <summary>PRJ001-PRJ008. Rules that read Player Settings have a pure Evaluate; a few tests also go through Run
    /// with the real settings, and always restore them.</summary>
    public class ProjectRulesTests
    {
        const string ManifestWithoutExported =
            "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\">\n" +
            "  <application>\n" +
            "    <activity android:name=\"com.example.Main\">\n" +
            "      <intent-filter><action android:name=\"android.intent.action.MAIN\" /></intent-filter>\n" +
            "    </activity>\n" +
            "  </application>\n" +
            "</manifest>";

        AndroidArchitecture _architectures;
        AndroidSdkVersions _targetSdk;

        [SetUp] public void SetUp()
        {
            _architectures = PlayerSettings.Android.targetArchitectures;
            _targetSdk = PlayerSettings.Android.targetSdkVersion;
        }

        [TearDown] public void TearDown()
        {
            PlayerSettings.Android.targetArchitectures = _architectures;
            PlayerSettings.Android.targetSdkVersion = _targetSdk;
        }

        static RuleContext Context()
        {
            var ctx = TestContexts.Create();
            ctx.Play = PlayRequirements.Parse(@"{ ""minTargetSdk"": 36, ""minTargetSdkDeadline"": ""2026-08-31"", ""require64Bit"": true }");
            return ctx;
        }

        // --- PRJ001 ---

        [Test] public void Prj001_TargetBelowMinimum_IsErrorWithFix()
        {
            var f = TargetApiRule.Evaluate(Context(), 34).Single();
            Assert.AreEqual("PRJ001", f.RuleId);
            Assert.AreEqual(Severity.Error, f.Severity);
            Assert.AreEqual("FIX_TARGET_API", f.FixId);
            StringAssert.Contains("36", f.FixLabel);
            StringAssert.Contains("2026-08-31", f.Evidence[2]);
        }

        [TestCase(36)]
        [TestCase(37)]
        public void Prj001_TargetAtOrAboveMinimum_NoFinding(int target)
        {
            Assert.IsEmpty(TargetApiRule.Evaluate(Context(), target));
        }

        [Test] public void Prj001_AutoResolvesToHighestInstalledPlatform()
        {
            using (var tmp = new TempDirectory())
            {
                var ctx = Context();
                ctx.Tools.Sdk = TestContexts.Tool(tmp.Root, ToolSource.Custom);

                tmp.Dir("platforms/android-34");
                Assert.AreEqual(Severity.Error, TargetApiRule.Evaluate(ctx, 0).Single().Severity);

                tmp.Dir("platforms/android-37.0");
                Assert.IsEmpty(TargetApiRule.Evaluate(ctx, 0));
            }
        }

        [Test] public void Prj001_AutoWithUnknownSdk_IsSkipped()
        {
            Assert.IsEmpty(TargetApiRule.Evaluate(Context(), 0));
        }

        [Test] public void Prj001_NoPlayData_IsSkipped()
        {
            var ctx = Context();
            ctx.Play = null;
            Assert.IsEmpty(TargetApiRule.Evaluate(ctx, 30));
        }

        [Test] public void Prj001_RunReadsTheRealTargetApiLevel()
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            Assert.IsEmpty(new TargetApiRule().Run(Context()));
        }

        // --- PRJ002 ---

        [Test] public void Prj002_MinBelowUnityMinimum_IsErrorWithFix()
        {
            var f = MinApiRule.Evaluate(TestContexts.Create(), 21).Single();
            Assert.AreEqual("PRJ002", f.RuleId);
            Assert.AreEqual(Severity.Error, f.Severity);
            Assert.AreEqual("FIX_MIN_API", f.FixId);
        }

        [Test] public void Prj002_MinAtUnityMinimum_NoFinding()
        {
            Assert.IsEmpty(MinApiRule.Evaluate(TestContexts.Create(), 26));
        }

        [Test] public void Prj002_UnityMinimumUnknown_IsSkipped()
        {
            var ctx = TestContexts.Create(@"{ ""unity"": [ { ""match"": ""6000"", ""minSdk"": -1 } ] }");
            Assert.IsEmpty(MinApiRule.Evaluate(ctx, 21));
        }

        [Test] public void Prj002_NoCompatRow_IsSkipped()
        {
            var ctx = TestContexts.Create();
            ctx.Compat = null;
            Assert.IsEmpty(MinApiRule.Evaluate(ctx, 21));
        }

        // --- PRJ003 ---

        [Test] public void Prj003_NoArm64_IsErrorWithFix()
        {
            var f = Arm64Il2CppRule.Evaluate(Context(), false, true, "ARMv7", "IL2CPP").Single();
            Assert.AreEqual("PRJ003", f.RuleId);
            Assert.AreEqual(Severity.Error, f.Severity);
            Assert.AreEqual("FIX_ARM64", f.FixId);
        }

        [Test] public void Prj003_Arm64WithMono_IsError()
        {
            var f = Arm64Il2CppRule.Evaluate(Context(), true, false, "ARM64", "Mono2x").Single();
            Assert.AreEqual(Severity.Error, f.Severity);
            Assert.AreEqual("FIX_ARM64", f.FixId);
        }

        [Test] public void Prj003_Arm64WithIl2Cpp_NoFinding()
        {
            Assert.IsEmpty(Arm64Il2CppRule.Evaluate(Context(), true, true, "ARM64", "IL2CPP"));
        }

        [Test] public void Prj003_64BitNotRequired_NoFinding()
        {
            var ctx = Context();
            ctx.Play = PlayRequirements.Parse(@"{ ""require64Bit"": false }");
            Assert.IsEmpty(Arm64Il2CppRule.Evaluate(ctx, false, false, "ARMv7", "Mono2x"));
        }

        [Test] public void Prj003_RunReadsTheRealArchitectures()
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            Assert.AreEqual(1, new Arm64Il2CppRule().Run(Context()).Count());

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            var findings = new Arm64Il2CppRule().Run(Context()).ToArray();
            Assert.IsEmpty(findings); // this project uses IL2CPP
        }

        // --- PRJ004 ---

        [Test] public void Prj004_AppBundleOff_IsInfo()
        {
            var f = AppBundleRule.Evaluate(false).Single();
            Assert.AreEqual("PRJ004", f.RuleId);
            Assert.AreEqual(Severity.Info, f.Severity);
        }

        [Test] public void Prj004_AppBundleOn_NoFinding()
        {
            Assert.IsEmpty(AppBundleRule.Evaluate(true));
        }

        // --- PRJ005 ---

        static RuleContext ContextWithAssets(TempDirectory tmp)
        {
            var ctx = Context();
            ctx.ProjectPath = tmp.Root;
            ctx.AssetsPath = tmp.Dir("Assets");
            return ctx;
        }

        [Test] public void Prj005_CustomTemplatesOnUnity65OrNewer_IsWarning()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/mainTemplate.gradle");
                tmp.File("Assets/Plugins/Android/gradleTemplate.properties");
                var f = GradleTemplatesRule.Evaluate(ContextWithAssets(tmp)).Single();
                Assert.AreEqual(Severity.Warning, f.Severity);
                CollectionAssert.AreEquivalent(new[]
                {
                    "Assets/Plugins/Android/mainTemplate.gradle",
                    "Assets/Plugins/Android/gradleTemplate.properties",
                }, f.Evidence);
            }
        }

        [Test] public void Prj005_CustomTemplatesOnOlderUnity_IsInfo()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/launcherTemplate.gradle");
                var ctx = ContextWithAssets(tmp);
                ctx.Unity = UnityVersion.Parse("2022.3.61f1");
                Assert.AreEqual(Severity.Info, GradleTemplatesRule.Evaluate(ctx).Single().Severity);
            }
        }

        [Test] public void Prj005_NoTemplates_NoFinding()
        {
            using (var tmp = new TempDirectory())
                Assert.IsEmpty(GradleTemplatesRule.Evaluate(ContextWithAssets(tmp)));
        }

        // --- PRJ006 ---

        [Test] public void Prj006_MainManifestMissingExported_IsErrorWithPathAndLine()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/AndroidManifest.xml", ManifestWithoutExported);
                var f = ManifestExportedRule.Evaluate(ContextWithAssets(tmp), 36).Single();
                Assert.AreEqual("PRJ006", f.RuleId);
                Assert.AreEqual(Severity.Error, f.Severity);
                StringAssert.Contains("Assets/Plugins/Android/AndroidManifest.xml", f.Evidence[0]);
                StringAssert.Contains("3", f.Evidence[0]);
                StringAssert.Contains("com.example.Main", f.Evidence[0]);
            }
        }

        [Test] public void Prj006_ManifestInsideAndroidLib_IsChecked_OthersAreIgnored()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/MyPlugin.androidlib/AndroidManifest.xml", ManifestWithoutExported);
                tmp.File("Assets/Other/AndroidManifest.xml", ManifestWithoutExported);
                var findings = ManifestExportedRule.Evaluate(ContextWithAssets(tmp), 36).ToArray();
                Assert.AreEqual(1, findings.Length);
                StringAssert.Contains("MyPlugin.androidlib", findings[0].Evidence[0]);
            }
        }

        [Test] public void Prj006_TargetBelow31_IsSkipped()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/AndroidManifest.xml", ManifestWithoutExported);
                Assert.IsEmpty(ManifestExportedRule.Evaluate(ContextWithAssets(tmp), 30));
            }
        }

        [Test] public void Prj006_AutoWithUnknownSdk_AssumesCurrentTarget()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/AndroidManifest.xml", ManifestWithoutExported);
                Assert.AreEqual(1, ManifestExportedRule.Evaluate(ContextWithAssets(tmp), 0).Count());
            }
        }

        [Test] public void Prj006_ExplicitExported_NoFinding()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/AndroidManifest.xml", ManifestWithoutExported.Replace(
                    "<activity android:name=\"com.example.Main\">",
                    "<activity android:name=\"com.example.Main\" android:exported=\"true\">"));
                Assert.IsEmpty(ManifestExportedRule.Evaluate(ContextWithAssets(tmp), 36));
            }
        }

        // --- PRJ007 ---

        [Test] public void Prj007_SameLibraryInTwoVersions_IsWarningWithSelectFix()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/foo-1.0.0.aar");
                tmp.File("Assets/Plugins/Android/foo-1.1.0.aar");
                var f = DuplicateLibrariesRule.Evaluate(ContextWithAssets(tmp)).Single();
                Assert.AreEqual("PRJ007", f.RuleId);
                Assert.AreEqual(Severity.Warning, f.Severity);
                // One button per version, newest first, and a last one to look at the files. None is part of "Fix all".
                Assert.AreEqual(3, f.Actions.Count);
                Assert.AreEqual("FIX_DISABLE_PLUGIN", f.Actions[0].FixId);
                Assert.AreEqual("Assets/Plugins/Android/foo-1.0.0.aar", f.Actions[0].Arg, "keeping 1.1.0 turns 1.0.0 off");
                Assert.AreEqual("Assets/Plugins/Android/foo-1.1.0.aar", f.Actions[1].Arg);
                Assert.AreEqual("FIX_SELECT_ASSET", f.Actions[2].FixId);
                Assert.IsFalse(f.Actions.Any(a => a.AllInOne));
                CollectionAssert.AreEqual(new[]
                {
                    "Assets/Plugins/Android/foo-1.0.0.aar",
                    "Assets/Plugins/Android/foo-1.1.0.aar",
                }, f.Evidence);
                StringAssert.Contains("foo", f.Title);
            }
        }

        [Test] public void Prj007_SameVersionTwice_UnversionedAndDifferentLibraries_AreNotReported()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/A/foo-1.0.0.aar");
                tmp.File("Assets/B/foo-1.0.0.aar");   // same version twice
                tmp.File("Assets/A/bar.aar");
                tmp.File("Assets/B/bar.aar");          // no version at all
                tmp.File("Assets/A/baz-1.0.0.aar");
                tmp.File("Assets/A/qux-2.0.0.aar");    // different libraries
                Assert.IsEmpty(DuplicateLibrariesRule.Evaluate(ContextWithAssets(tmp)));
            }
        }

        [Test] public void Prj007_LibrariesInIgnoredFolders_AreNotCounted()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/foo-1.0.0.aar");
                tmp.File("Assets/Samples~/foo-2.0.0.aar");
                Assert.IsEmpty(DuplicateLibrariesRule.Evaluate(ContextWithAssets(tmp)));
            }
        }

        // --- PRJ008 ---

        [Test] public void Prj008_ReleaseWithoutCustomKeystore_IsWarning()
        {
            var f = ReleaseSigningRule.Evaluate(false, false).Single();
            Assert.AreEqual("PRJ008", f.RuleId);
            Assert.AreEqual(Severity.Warning, f.Severity);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void Prj008_CustomKeystoreOrDevelopmentBuild_NoFinding(bool keystore, bool development)
        {
            Assert.IsEmpty(ReleaseSigningRule.Evaluate(keystore, development));
        }
    }
}
