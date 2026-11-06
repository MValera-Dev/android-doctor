using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Environment;

namespace AndroidDoctor.Tests
{
    /// <summary>ENV003, ENV004 and ENV005: the Android SDK folder, platforms and build-tools.</summary>
    public class SdkRulesTests
    {
        static RuleContext ContextWithSdk(string path)
        {
            var ctx = TestContexts.Create();
            ctx.Tools.Sdk = TestContexts.Tool(path, ToolSource.Custom);
            return ctx;
        }

        // --- ENV003 ---

        [Test] public void Env003_MissingSdkFolder_IsError()
        {
            var f = SdkLayoutRule.Evaluate(ContextWithSdk("Z:/does/not/exist/sdk")).Single();
            Assert.AreEqual("ENV003", f.RuleId);
            Assert.AreEqual(Severity.Error, f.Severity);
        }

        [Test] public void Env003_UnknownSdkPath_IsSkipped()
        {
            var ctx = TestContexts.Create();
            ctx.Tools.Sdk = TestContexts.Tool(null, ToolSource.Unknown);
            Assert.IsEmpty(SdkLayoutRule.Evaluate(ctx));
        }

        [Test] public void Env003_MissingPlatformTools_IsError()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("cmdline-tools/16.0");
                var f = SdkLayoutRule.Evaluate(ContextWithSdk(tmp.Root)).Single();
                Assert.AreEqual(Severity.Error, f.Severity);
                StringAssert.Contains("platform-tools", f.Detail);
            }
        }

        [Test] public void Env003_MissingCommandLineTools_IsError()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("platform-tools");
                var f = SdkLayoutRule.Evaluate(ContextWithSdk(tmp.Root)).Single();
                StringAssert.Contains("cmdline-tools", f.Detail);
            }
        }

        [Test] public void Env003_CompleteSdk_NoFinding()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("platform-tools");
                tmp.Dir("cmdline-tools/16.0");
                Assert.IsEmpty(SdkLayoutRule.Evaluate(ContextWithSdk(tmp.Root)));
            }
        }

        // --- ENV004 ---

        [Test] public void Env004_MissingTargetPlatform_IsErrorWithSdkManagerCommand()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("platforms/android-34");
                var f = SdkPlatformRule.Evaluate(ContextWithSdk(tmp.Root), 36).Single();
                Assert.AreEqual("ENV004", f.RuleId);
                Assert.AreEqual(Severity.Error, f.Severity);
                Assert.AreEqual("sdkmanager \"platforms;android-36\"", f.Command);
                StringAssert.Contains("android-34", f.Evidence[1]);
            }
        }

        [Test] public void Env004_TargetPlatformInstalled_NoFinding()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("platforms/android-36");
                Assert.IsEmpty(SdkPlatformRule.Evaluate(ContextWithSdk(tmp.Root), 36));
            }
        }

        [Test] public void Env004_PlatformWithMinorSuffix_Counts()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("platforms/android-37.0");
                Assert.IsEmpty(SdkPlatformRule.Evaluate(ContextWithSdk(tmp.Root), 37));
            }
        }

        [Test] public void Env004_AutoWithAnyPlatform_NoFinding()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("platforms/android-34");
                Assert.IsEmpty(SdkPlatformRule.Evaluate(ContextWithSdk(tmp.Root), 0));
            }
        }

        [Test] public void Env004_AutoWithNoPlatforms_IsError()
        {
            using (var tmp = new TempDirectory())
            {
                var f = SdkPlatformRule.Evaluate(ContextWithSdk(tmp.Root), 0).Single();
                Assert.AreEqual(Severity.Error, f.Severity);
                Assert.IsNull(f.Command); // no Play data in this context, so no suggested API level
            }
        }

        [Test] public void Env004_AutoWithNoPlatforms_SuggestsPlayMinimumWhenKnown()
        {
            using (var tmp = new TempDirectory())
            {
                var ctx = ContextWithSdk(tmp.Root);
                ctx.Play = PlayRequirements.Parse(@"{ ""minTargetSdk"": 36 }");
                var f = SdkPlatformRule.Evaluate(ctx, 0).Single();
                Assert.AreEqual("sdkmanager \"platforms;android-36\"", f.Command);
            }
        }

        [Test] public void Env004_MissingSdkFolder_IsLeftToEnv003()
        {
            Assert.IsEmpty(SdkPlatformRule.Evaluate(ContextWithSdk("Z:/does/not/exist/sdk"), 36));
        }

        // --- ENV005 ---

        [Test] public void Env005_ExpectedBuildToolsMissing_IsInfo()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("build-tools/34.0.0");
                var f = BuildToolsRule.Evaluate(ContextWithSdk(tmp.Root)).Single();
                Assert.AreEqual("ENV005", f.RuleId);
                Assert.AreEqual(Severity.Info, f.Severity);
                StringAssert.Contains("36.0.0", f.Title);
            }
        }

        [Test] public void Env005_ExpectedBuildToolsInstalled_NoFinding()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("build-tools/36.0.0");
                Assert.IsEmpty(BuildToolsRule.Evaluate(ContextWithSdk(tmp.Root)));
            }
        }

        [Test] public void Env005_ExpectedBuildToolsUnknown_IsSkipped()
        {
            using (var tmp = new TempDirectory())
            {
                var ctx = ContextWithSdk(tmp.Root);
                ctx.Compat = CompatTable.Parse(@"{ ""unity"": [ { ""match"": ""6000"", ""buildTools"": null } ] }");
                Assert.IsEmpty(BuildToolsRule.Evaluate(ctx));
            }
        }
    }
}
