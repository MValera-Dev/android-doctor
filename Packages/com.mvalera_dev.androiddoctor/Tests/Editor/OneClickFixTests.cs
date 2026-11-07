using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using AndroidDoctor.Environment;
using AndroidDoctor.Project;
using AndroidDoctor.UI;

namespace AndroidDoctor.Tests
{
    /// <summary>Findings with several actions, "Fix all", and the fixes behind the one-click buttons.</summary>
    public class OneClickFixTests
    {
        bool _appBundle;

        [SetUp] public void SetUp() { _appBundle = EditorUserBuildSettings.buildAppBundle; }
        [TearDown] public void TearDown() { EditorUserBuildSettings.buildAppBundle = _appBundle; }

        static Finding WithActions(string id, params FixAction[] actions)
        {
            return new Finding(id, Category.Project, Severity.Warning, "t", "d", actions: actions);
        }

        // ---- the finding model ---------------------------------------------------------------------------

        [Test] public void ALegacyFixId_BecomesOneAction_AndFixIdStillReadsAsBefore()
        {
            var f = new Finding("PRJ001", Category.Project, Severity.Error, "t", "d", null, FixIds.TargetApi, "Fix it");
            Assert.AreEqual(1, f.Actions.Count);
            Assert.AreEqual(FixIds.TargetApi, f.FixId);
            Assert.AreEqual("Fix it", f.FixLabel);
            Assert.IsTrue(f.Actions[0].AllInOne, "changing a setting is part of Fix all");
        }

        [Test] public void ALegacyFixThatOnlyOpensSomething_IsNeverPartOfFixAll()
        {
            var f = new Finding("PRJ007", Category.Project, Severity.Warning, "t", "d", null, FixIds.SelectAsset, "Show");
            Assert.IsFalse(f.Actions[0].AllInOne);
        }

        [Test] public void AFindingWithoutActions_HasNoFixId()
        {
            var f = new Finding("X", Category.Project, Severity.Info, "t", "d");
            Assert.IsNull(f.FixId);
            Assert.AreEqual(0, f.Actions.Count);
        }

        // ---- Fix all -------------------------------------------------------------------------------------

        [Test] public void FixAll_CountsEachChangeOnce_EvenWhenTwoFindingsAskForIt()
        {
            var a = WithActions("A", new FixAction(FixIds.AppBundle, "x", null, true));
            var b = WithActions("B", new FixAction(FixIds.AppBundle, "x", null, true));
            Assert.AreEqual(1, FixFlow.CountAllInOne(new[] { a, b }));
        }

        [Test] public void FixAll_LeavesOutChoices()
        {
            var choice = WithActions("A", new FixAction(FixIds.AppBundle, "x")); // not AllInOne
            Assert.AreEqual(0, FixFlow.CountAllInOne(new[] { choice }));
        }

        [Test] public void FixAll_AsksOnceWithEveryChange_AndAppliesNothingIfRefused()
        {
            EditorUserBuildSettings.buildAppBundle = false;
            string shown = null;
            string message;
            var findings = new[] { WithActions("A", new FixAction(FixIds.AppBundle, "x", null, true)) };

            bool applied;
            using (Messages.English()) applied = FixFlow.RunAll(findings, text => { shown = text; return false; }, out message);

            Assert.IsFalse(applied);
            StringAssert.Contains("1", shown);
            StringAssert.Contains("App Bundle", shown);
            Assert.IsFalse(EditorUserBuildSettings.buildAppBundle, "refusing changes nothing");
        }

        [Test] public void FixAll_AppliesAndReportsHowManyWorked()
        {
            EditorUserBuildSettings.buildAppBundle = false;
            string message;
            var findings = new[] { WithActions("A", new FixAction(FixIds.AppBundle, "x", null, true)) };

            bool applied;
            using (Messages.English()) applied = FixFlow.RunAll(findings, text => true, out message);

            Assert.IsTrue(applied);
            Assert.IsTrue(EditorUserBuildSettings.buildAppBundle);
            StringAssert.Contains("1 of 1", message);
        }

        [Test] public void FixAll_WithNothingToDo_IsFalse_AndNeverAsks()
        {
            bool asked = false;
            string message;
            Assert.IsFalse(FixFlow.RunAll(new Finding[0], text => { asked = true; return true; }, out message));
            Assert.IsFalse(asked);
        }

        [Test] public void AnActionWhoseFixDoesNotExist_IsIgnoredByFixAll()
        {
            var f = WithActions("A", new FixAction("FIX_NOBODY_HAS_THIS", "x", null, true));
            Assert.AreEqual(0, FixFlow.CountAllInOne(new[] { f }));
        }

        // ---- tools installed with Unity ------------------------------------------------------------------

        [Test] public void UnityTools_WhenEverythingIsAlreadyUnitys_SaysSoAndChangesNothing()
        {
            var fix = new UseUnityToolsFix();
            if (!ExternalToolsSettings.Kinds.All(ExternalToolsSettings.IsEmbedded))
                Assert.Ignore("This machine uses a custom tool, so there is something to change.");
            string message;
            using (Messages.English()) Assert.IsFalse(fix.Apply(WithActions("A"), null, out message));
            StringAssert.Contains("already", message);
        }

        [Test] public void UnityTools_OfferedOnlyForACustomTool()
        {
            Assert.AreEqual(1, UseUnityToolsFix.ActionsFor("jdk", false).Length);
            var custom = UseUnityToolsFix.ActionsFor("jdk", true);
            Assert.AreEqual(2, custom.Length);
            Assert.AreEqual("jdk", custom[0].Arg);
            Assert.IsTrue(custom[0].AllInOne);
        }

        // ---- Gradle heap ---------------------------------------------------------------------------------

        [TestCase(1024, 16000, 4096)]   // at least 4 GB
        [TestCase(4096, 16000, 8192)]   // doubled
        [TestCase(8192, 32000, 8192)]   // never above 8 GB
        [TestCase(4096, 6000, 3600)]    // never above 60% of the machine
        [TestCase(1024, 2000, 2048)]    // but a small machine still gets the healthy minimum
        [TestCase(2048, 0, 4096)]       // unknown memory
        public void RaiseHeap_Target(int current, int system, int expected)
        {
            Assert.AreEqual(expected, RaiseHeapFix.Target(current, system));
        }

        [Test] public void Env010_ASmallHeap_OffersToRaiseIt_InFixAll()
        {
            var f = GradleHeapRule.Evaluate(1024).Single();
            Assert.AreEqual("ENV010", f.RuleId);
            Assert.AreEqual(FixIds.Heap, f.Actions[0].FixId);
            Assert.IsTrue(f.Actions[0].AllInOne);
            StringAssert.Contains("1024", f.Title);
        }

        [TestCase(2048)]
        [TestCase(4096)]
        [TestCase(-1)]
        public void Env010_AHealthyOrUnknownHeap_IsSilent(int heap)
        {
            Assert.AreEqual(0, GradleHeapRule.Evaluate(heap).Count());
        }

        // ---- environment variables -----------------------------------------------------------------------

        [Test] public void ClearEnvironmentVariable_RemovesItFromTheProcessOnly()
        {
            const string name = "ANDROID_DOCTOR_TEST_VARIABLE";
            System.Environment.SetEnvironmentVariable(name, "value");
            try
            {
                var fix = new ClearEnvironmentVariableFix();
                var action = new FixAction(FixIds.ClearEnvironmentVariable, "x", name, true);
                StringAssert.Contains(name, fix.Describe(WithActions("A"), action));

                string message;
                Assert.IsTrue(fix.Apply(WithActions("A"), action, out message));
                Assert.IsNull(System.Environment.GetEnvironmentVariable(name));
                Assert.IsFalse(fix.Apply(WithActions("A"), action, out message), "nothing left to remove");
            }
            finally { System.Environment.SetEnvironmentVariable(name, null); }
        }

        [Test] public void Env008_TheSdkHomeWarning_IsOneClick_TheOthersAreOnlyInformation()
        {
            var findings = EnvironmentVariablesRule.Evaluate(n => n == "ANDROID_SDK_HOME" || n == "GRADLE_USER_HOME" ? "C:/x" : null).ToList();
            var sdkHome = findings.Single(f => f.Severity == Severity.Warning);
            Assert.AreEqual("ANDROID_SDK_HOME", sdkHome.Actions[0].Arg);
            Assert.IsTrue(sdkHome.Actions[0].AllInOne);
            Assert.AreEqual(0, findings.Single(f => f.Severity == Severity.Info).Actions.Count);
        }

        // ---- App Bundle, packages, settings --------------------------------------------------------------

        [Test] public void AppBundle_IsAChoice_NotPartOfFixAll()
        {
            var f = AppBundleRule.Evaluate(false).Single();
            Assert.AreEqual(FixIds.AppBundle, f.FixId);
            Assert.IsFalse(f.Actions[0].AllInOne, "an .aab cannot be installed on a phone directly");
        }

        [Test] public void AppBundleFix_TurnsItOn()
        {
            EditorUserBuildSettings.buildAppBundle = false;
            string message;
            Assert.IsTrue(new EnableAppBundleFix().Apply(WithActions("A"), out message));
            Assert.IsTrue(EditorUserBuildSettings.buildAppBundle);
        }

        // ---- duplicate libraries -------------------------------------------------------------------------

        [Test] public void DuplicateLibraries_AreOrderedByRealVersionNumbers_NotAsText()
        {
            var files = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>
            {
                new System.Collections.Generic.KeyValuePair<string, string>("Assets/a-1.9.0.aar", "1.9.0"),
                new System.Collections.Generic.KeyValuePair<string, string>("Assets/a-1.10.0.aar", "1.10.0"),
            };
            var actions = DuplicateLibrariesRule.KeepOneVersion(files, new System.Collections.Generic.List<string> { "1.10.0", "1.9.0" });
            Assert.AreEqual("Assets/a-1.9.0.aar", actions[0].Arg, "1.10.0 is newer than 1.9.0, so keeping it turns 1.9.0 off");
        }

        [Test] public void DisablePlugin_TurnsTheFileOffForAndroid_AndLeavesItInTheProject()
        {
            const string folder = "Assets/AndroidDoctorTestPlugins";
            const string path = folder + "/dup-1.0.0.jar";
            try
            {
                Directory.CreateDirectory(folder);
                File.WriteAllBytes(path, new byte[] { 0x50, 0x4B, 0x05, 0x06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (PluginImporter)AssetImporter.GetAtPath(path);
                importer.SetCompatibleWithPlatform(BuildTarget.Android, true);
                importer.SaveAndReimport();
                Assert.IsTrue(importer.GetCompatibleWithPlatform(BuildTarget.Android));

                string message;
                var fix = new DisablePluginFix();
                var action = new FixAction(FixIds.DisablePlugin, "x", path);
                StringAssert.Contains(path, fix.Describe(WithActions("A"), action));
                Assert.IsTrue(fix.Apply(WithActions("A"), action, out message), message);

                importer = (PluginImporter)AssetImporter.GetAtPath(path);
                Assert.IsFalse(importer.GetCompatibleWithPlatform(BuildTarget.Android));
                Assert.IsTrue(File.Exists(path), "the file itself stays");
            }
            finally { AssetDatabase.DeleteAsset(folder); }
        }

        [Test] public void DisablePlugin_AFileThatIsNotThere_IsReportedNotInvented()
        {
            string message;
            Assert.IsFalse(new DisablePluginFix().Apply(WithActions("A"), new FixAction(FixIds.DisablePlugin, "x", "Assets/nope.jar"), out message));
            StringAssert.Contains("nope.jar", message);
        }

        // ---- android:exported ----------------------------------------------------------------------------

        const string Manifest =
            "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\">\n" +
            "  <application>\n" +
            "    <activity android:name=\"com.example.Main\">\n" +
            "      <intent-filter><action android:name=\"android.intent.action.MAIN\" /><category android:name=\"android.intent.category.LAUNCHER\" /></intent-filter>\n" +
            "    </activity>\n" +
            "    <activity android:name=\"com.example.Link\">\n" +
            "      <intent-filter><action android:name=\"android.intent.action.VIEW\" /></intent-filter>\n" +
            "    </activity>\n" +
            "    <receiver android:name=\"com.example.Boot\">\n" +
            "      <intent-filter><action android:name=\"android.intent.action.BOOT_COMPLETED\" /></intent-filter>\n" +
            "    </receiver>\n" +
            "    <service android:name=\"com.vendor.Push\">\n" +
            "      <intent-filter><action android:name=\"com.vendor.PUSH\" /></intent-filter>\n" +
            "    </service>\n" +
            "  </application>\n" +
            "</manifest>";

        [Test] public void Exported_ChoosesTheValueFromWhatTheComponentIs()
        {
            var found = ManifestExportedChecker.FindMissing(Manifest).ToDictionary(c => c.Name);
            Assert.IsTrue(found["com.example.Main"].SuggestedExported);
            Assert.AreEqual("launcher", found["com.example.Main"].Reason);
            Assert.IsTrue(found["com.example.Link"].SuggestedExported);
            Assert.IsFalse(found["com.example.Boot"].SuggestedExported, "system broadcasts reach it either way");
            Assert.AreEqual("private", found["com.example.Boot"].Reason);
            Assert.IsTrue(found["com.vendor.Push"].SuggestedExported, "a vendor action must come from outside");
        }

        [Test] public void Exported_TryAdd_WritesTheAttributes_KeepsEverythingElse_AndLeavesValidXml()
        {
            string edited;
            int count;
            Assert.IsTrue(SetExportedFix.TryAdd(Manifest, out edited, out count));
            Assert.AreEqual(4, count);
            StringAssert.Contains("<activity android:exported=\"true\" android:name=\"com.example.Main\">", edited);
            StringAssert.Contains("<receiver android:exported=\"false\" android:name=\"com.example.Boot\">", edited);
            Assert.AreEqual(0, ManifestExportedChecker.FindMissing(edited).Count, "nothing is missing afterwards");
            Assert.AreEqual(Manifest.Split('\n').Length, edited.Split('\n').Length, "no line was added or lost");
        }

        [Test] public void Exported_TryAdd_KeepsWindowsLineEndings()
        {
            string edited;
            int count;
            SetExportedFix.TryAdd(Manifest.Replace("\n", "\r\n"), out edited, out count);
            StringAssert.Contains("\r\n", edited);
            Assert.IsFalse(edited.Replace("\r\n", "").Contains("\n"), "no bare line feed was introduced");
        }

        [Test] public void Exported_TryAdd_ARunWithNothingMissing_ChangesNothing()
        {
            string first, second;
            int count;
            SetExportedFix.TryAdd(Manifest, out first, out count);
            SetExportedFix.TryAdd(first, out second, out count);
            Assert.AreEqual(0, count);
            Assert.AreEqual(first, second);
        }

        [Test] public void Exported_TryAdd_RefusesAManifestWithoutTheAndroidPrefix()
        {
            string edited;
            int count;
            Assert.IsFalse(SetExportedFix.TryAdd("<manifest xmlns:a=\"http://schemas.android.com/apk/res/android\"><application><activity a:name=\"X\"><intent-filter/></activity></application></manifest>", out edited, out count));
        }

        [Test] public void Exported_Apply_EditsTheFile_AndKeepsABackup()
        {
            const string folder = "Assets/AndroidDoctorTestManifest";
            const string path = folder + "/AndroidManifest.xml";
            try
            {
                Directory.CreateDirectory(folder);
                File.WriteAllText(path, Manifest);
                string message;
                var fix = new SetExportedFix();
                var action = new FixAction(FixIds.Exported, "x", path, true);
                StringAssert.Contains("com.vendor.Push", fix.Describe(WithActions("A"), action));

                Assert.IsTrue(fix.Apply(WithActions("A"), action, out message), message);

                Assert.AreEqual(0, ManifestExportedChecker.FindMissing(File.ReadAllText(path)).Count);
                string backup = Path.Combine(FixBackup.RunFolder, path.Replace('/', Path.DirectorySeparatorChar));
                Assert.IsTrue(File.Exists(backup), "the original was copied first");
                Assert.AreEqual(Manifest, File.ReadAllText(backup));
            }
            finally { AssetDatabase.DeleteAsset(folder); }
        }

        // ---- gradleTemplate.properties -------------------------------------------------------------------

        [Test] public void GradleProperties_ReplacesTheRemovedSettingOnce_AndKeepsTheRest()
        {
            string text = "org.gradle.jvmargs=-Xmx4096M\n# keep\nandroid.enableDexingArtifactTransform=false\nandroid.enableDexingArtifactTransform.desugaring=false\nandroid.useAndroidX=true\n";
            int changed;
            string result = GradleProperties.Migrate(text, out changed);
            Assert.AreEqual(2, changed);
            Assert.AreEqual("org.gradle.jvmargs=-Xmx4096M\n# keep\nandroid.useFullClasspathForDexingTransform=true\nandroid.useAndroidX=true\n", result);
        }

        [Test] public void GradleProperties_DoesNotDuplicateTheReplacement()
        {
            int changed;
            string result = GradleProperties.Migrate("android.useFullClasspathForDexingTransform=true\nandroid.enableDexingArtifactTransform=false\n", out changed);
            Assert.AreEqual(1, changed);
            Assert.AreEqual("android.useFullClasspathForDexingTransform=true\n", result);
        }

        [Test] public void GradleProperties_AFileWithoutTheProblem_IsUntouched()
        {
            int changed;
            string text = "org.gradle.jvmargs=-Xmx4096M\r\nandroid.useAndroidX=true\r\n";
            Assert.AreEqual(text, GradleProperties.Migrate(text, out changed));
            Assert.AreEqual(0, changed);
        }

        [Test] public void GradleProperties_KeepsWindowsLineEndings()
        {
            int changed;
            string result = GradleProperties.Migrate("a=1\r\nandroid.enableDexingArtifactTransform=false\r\nb=2\r\n", out changed);
            Assert.AreEqual("a=1\r\nandroid.useFullClasspathForDexingTransform=true\r\nb=2\r\n", result);
        }

        [Test] public void GradleProperties_ACommentedOutLine_IsNotAProblem()
        {
            Assert.AreEqual(0, GradleProperties.FindRemoved("# android.enableDexingArtifactTransform=false").Count);
        }

        [Test] public void Prj010_FindsTheRemovedSetting_BeforeTheBuild_AndOffersTheFix()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/gradleTemplate.properties", "android.enableDexingArtifactTransform=false\n");
                var ctx = TestContexts.Create();
                ctx.ProjectPath = tmp.Root;
                ctx.AssetsPath = tmp.Dir("Assets");

                var f = RemovedGradlePropertiesRule.Evaluate(ctx).Single();

                Assert.AreEqual("PRJ010", f.RuleId);
                Assert.AreEqual(Severity.Error, f.Severity);
                Assert.AreEqual(FixIds.GradleProperties, f.Actions[0].FixId);
                Assert.IsTrue(f.Actions[0].AllInOne);
                Assert.AreEqual(FixIds.ResetTemplates, f.Actions[1].FixId);
                Assert.IsFalse(f.Actions[1].AllInOne);
            }
        }

        [Test] public void Prj010_OnUnity2022_ThePropertyIsStillValid()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/gradleTemplate.properties", "android.enableDexingArtifactTransform=false\n");
                var ctx = TestContexts.Create();
                ctx.Unity = UnityVersion.Parse("2022.3.61f1");
                ctx.ProjectPath = tmp.Root;
                ctx.AssetsPath = tmp.Dir("Assets");
                Assert.AreEqual(0, RemovedGradlePropertiesRule.Evaluate(ctx).Count());
            }
        }

        // ---- templates and build cache -------------------------------------------------------------------

        [Test] public void ResetTemplates_BacksUpAndRemoves()
        {
            const string folder = "Assets/AndroidDoctorTestTemplates";
            const string path = folder + "/mainTemplate.gradle";
            try
            {
                Directory.CreateDirectory(folder);
                File.WriteAllText(path, "// custom");
                string message;
                var fix = new ResetTemplatesFix();
                var action = new FixAction(FixIds.ResetTemplates, "x", path);
                Assert.IsTrue(fix.Apply(WithActions("A"), action, out message), message);
                Assert.IsFalse(File.Exists(path));
                Assert.IsTrue(File.Exists(Path.Combine(FixBackup.RunFolder, path.Replace('/', Path.DirectorySeparatorChar))));
            }
            finally { AssetDatabase.DeleteAsset(folder); }
        }

        [Test] public void ResetTemplates_WithNoFile_DoesNothing()
        {
            string message;
            Assert.IsFalse(new ResetTemplatesFix().Apply(WithActions("A"), new FixAction(FixIds.ResetTemplates, "x", "Assets/none.gradle"), out message));
        }

        [Test] public void Prj005_OffersToResetTheTemplatesItFound_ButNeverInFixAll()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/Plugins/Android/mainTemplate.gradle");
                var ctx = TestContexts.Create();
                ctx.ProjectPath = tmp.Root;
                ctx.AssetsPath = tmp.Dir("Assets");
                var f = GradleTemplatesRule.Evaluate(ctx).Single();
                Assert.AreEqual(FixIds.ResetTemplates, f.Actions[0].FixId);
                Assert.AreEqual("Assets/Plugins/Android/mainTemplate.gradle", f.Actions[0].Arg);
                Assert.IsFalse(f.Actions[0].AllInOne, "the user may have customised it on purpose");
            }
        }

        // ---- known packages ------------------------------------------------------------------------------

        [Test] public void KnownPackage_FromUnitysRegistry_CanBeUpdatedInOneClick_AnotherIsOnlyOpened()
        {
            var play = PlayRequirements.Parse(@"{ ""knownPackages"": [
                { ""name"": ""com.unity.burst"", ""minVersion"": ""1.8.21"", ""severity"": ""Error"", ""reason"": ""r"" },
                { ""name"": ""com.google.android.appbundle"", ""minVersion"": ""1.10.0"", ""severity"": ""Warning"", ""reason"": ""r"" } ] }");
            var ctx = TestContexts.Create();
            ctx.Play = play;
            ctx.Packages.Add(new PackageRef { Name = "com.unity.burst", Version = "1.8.1" });
            ctx.Packages.Add(new PackageRef { Name = "com.google.android.appbundle", Version = "1.9.0" });

            var burst = new Native.BurstVersionRule().Run(ctx).Single();
            var bundle = new Native.AppBundlePackageRule().Run(ctx).Single();

            Assert.AreEqual(FixIds.UpdatePackage, burst.Actions[0].FixId);
            Assert.AreEqual("com.unity.burst", burst.Actions[0].Arg);
            Assert.IsTrue(burst.Actions[0].AllInOne);
            Assert.AreEqual(FixIds.OpenPackage, bundle.Actions[0].FixId);
            Assert.IsFalse(bundle.Actions[0].AllInOne);
        }

        [Test] public void CleanBuildCache_WhenThereIsNothingToDelete_SaysSo()
        {
            var fix = new CleanBuildCacheFix();
            string message;
            if (Directory.Exists(Path.Combine(FixBackup.ProjectPath, "Library", "Bee", "Android"))
                || Directory.Exists(Path.Combine(FixBackup.ProjectPath, "Temp", "gradleOut")))
                Assert.Ignore("A generated Android project exists here; deleting it is left to the manual check.");
            using (Messages.English()) Assert.IsFalse(fix.Apply(WithActions("A"), out message));
            StringAssert.Contains("no generated", message.ToLowerInvariant());
        }

        // ---- every fix is reachable and described --------------------------------------------------------

        [Test] public void EveryFixId_HasAFix_ThatDescribesItselfWithoutThrowing()
        {
            foreach (string id in FixIds.All)
            {
                var fix = FixRegistry.Get(id);
                Assert.IsNotNull(fix, id);
                var action = new FixAction(id, "x");
                Assert.DoesNotThrow(() =>
                {
                    var actionFix = fix as IActionFix;
                    string text = actionFix != null ? actionFix.Describe(WithActions("A", action), action) : fix.Describe(WithActions("A", action));
                    Assert.IsNotEmpty(text, id);
                }, id);
            }
        }
    }
}
