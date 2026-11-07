using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Environment;

namespace AndroidDoctor.Tests
{
    /// <summary>ENV001, ENV002, ENV006 and ENV007: the module and the JDK, NDK and Gradle tools.</summary>
    public class ToolchainRulesTests
    {
        // --- ENV001 ---

        [Test] public void Env001_ModuleMissing_IsError()
        {
            var findings = AndroidModuleInstalledRule.Evaluate(TestContexts.Create(), false).ToArray();
            Assert.AreEqual(1, findings.Length);
            Assert.AreEqual("ENV001", findings[0].RuleId);
            Assert.AreEqual(Severity.Error, findings[0].Severity);
        }

        [Test] public void Env001_ModulePresent_NoFinding()
        {
            Assert.IsEmpty(AndroidModuleInstalledRule.Evaluate(TestContexts.Create(), true));
        }

        // --- ENV002 ---

        [Test] public void Env002_UnknownJdkPath_IsSkipped()
        {
            var ctx = TestContexts.Create();
            ctx.Tools.Jdk = TestContexts.Tool(null, ToolSource.Unknown);
            Assert.IsEmpty(JdkRule.Evaluate(ctx));
        }

        [Test] public void Env002_MissingFolder_IsErrorWithFix()
        {
            var ctx = TestContexts.Create();
            ctx.Tools.Jdk = TestContexts.Tool("Z:/does/not/exist/jdk", ToolSource.Custom);
            var f = JdkRule.Evaluate(ctx).Single();
            Assert.AreEqual(Severity.Error, f.Severity);
            Assert.AreEqual("FIX_UNITY_TOOLS", f.FixId, "a custom JDK that is missing is put back to Unity's own");
            Assert.AreEqual("jdk", f.Actions[0].Arg);
            Assert.IsTrue(f.Actions[0].AllInOne);
            Assert.AreEqual("FIX_OPEN_EXTERNAL_TOOLS", f.Actions[1].FixId, "and the user can still go and look");
        }

        [Test] public void Env002_MissingEmbeddedJdk_OnlyOffersToOpenThePreferences()
        {
            var ctx = TestContexts.Create();
            ctx.Tools.Jdk = TestContexts.Tool("Z:/does/not/exist/jdk", ToolSource.Embedded);
            var f = JdkRule.Evaluate(ctx).Single();
            Assert.AreEqual("FIX_OPEN_EXTERNAL_TOOLS", f.FixId, "switching to Unity's own tool changes nothing when it is the broken one");
            Assert.AreEqual(1, f.Actions.Count);
        }

        [Test] public void Env002_CustomJdkOfAnotherMajor_IsWarning()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("release", "JAVA_VERSION=\"21.0.2\"\n");
                var ctx = TestContexts.Create();
                ctx.Tools.Jdk = TestContexts.Tool(tmp.Root, ToolSource.Custom);
                var f = JdkRule.Evaluate(ctx).Single();
                Assert.AreEqual(Severity.Warning, f.Severity);
                StringAssert.Contains("21.0.2", f.Evidence[1]);
            }
        }

        [Test] public void Env002_CustomJdkOfExpectedMajor_NoFinding()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("release", "JAVA_VERSION=\"17.0.9\"\n");
                var ctx = TestContexts.Create();
                ctx.Tools.Jdk = TestContexts.Tool(tmp.Root, ToolSource.Custom);
                Assert.IsEmpty(JdkRule.Evaluate(ctx));
            }
        }

        [Test] public void Env002_EmbeddedJdk_IsNotJudged()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("release", "JAVA_VERSION=\"21.0.2\"\n");
                var ctx = TestContexts.Create();
                ctx.Tools.Jdk = TestContexts.Tool(tmp.Root, ToolSource.Embedded);
                Assert.IsEmpty(JdkRule.Evaluate(ctx));
            }
        }

        [Test] public void Env002_NoCompatData_IsSkipped()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("release", "JAVA_VERSION=\"21.0.2\"\n");
                var ctx = TestContexts.Create();
                ctx.Compat = null;
                ctx.Tools.Jdk = TestContexts.Tool(tmp.Root, ToolSource.Custom);
                Assert.IsEmpty(JdkRule.Evaluate(ctx));
            }
        }

        // --- ENV006 ---

        [Test] public void Env006_WithoutIl2Cpp_IsSkipped()
        {
            var ctx = TestContexts.Create();
            ctx.Tools.Ndk = TestContexts.Tool("Z:/does/not/exist/ndk", ToolSource.Custom);
            Assert.IsEmpty(NdkRule.Evaluate(ctx, false));
        }

        [Test] public void Env006_MissingFolder_IsError()
        {
            var ctx = TestContexts.Create();
            ctx.Tools.Ndk = TestContexts.Tool("Z:/does/not/exist/ndk", ToolSource.Custom);
            Assert.AreEqual(Severity.Error, NdkRule.Evaluate(ctx, true).Single().Severity);
        }

        [Test] public void Env006_CustomNdkOfAnotherRevision_IsWarning()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("source.properties", "Pkg.Revision = 25.1.8937393\n");
                var ctx = TestContexts.Create();
                ctx.Tools.Ndk = TestContexts.Tool(tmp.Root, ToolSource.Custom);
                Assert.AreEqual(Severity.Warning, NdkRule.Evaluate(ctx, true).Single().Severity);
            }
        }

        [Test] public void Env006_CustomNdkOfExpectedRevision_NoFinding()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("source.properties", "Pkg.Revision = 27.2.12479018\n");
                var ctx = TestContexts.Create();
                ctx.Tools.Ndk = TestContexts.Tool(tmp.Root, ToolSource.Custom);
                Assert.IsEmpty(NdkRule.Evaluate(ctx, true));
            }
        }

        [Test] public void Env006_EmbeddedNdk_IsNotJudged()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("source.properties", "Pkg.Revision = 25.1.8937393\n");
                var ctx = TestContexts.Create();
                ctx.Tools.Ndk = TestContexts.Tool(tmp.Root, ToolSource.Embedded);
                Assert.IsEmpty(NdkRule.Evaluate(ctx, true));
            }
        }

        // --- ENV007 ---

        [Test] public void Env007_CustomGradleOfAnotherVersion_IsWarning()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("lib/gradle-launcher-8.4.jar");
                var ctx = TestContexts.Create();
                ctx.Tools.Gradle = TestContexts.Tool(tmp.Root, ToolSource.Custom);
                var f = GradleRule.Evaluate(ctx).Single();
                Assert.AreEqual(Severity.Warning, f.Severity);
                StringAssert.Contains("8.4", f.Evidence[1]);
            }
        }

        [Test] public void Env007_CustomGradleOfExpectedVersion_NoFinding()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("lib/gradle-launcher-9.3.1.jar");
                var ctx = TestContexts.Create();
                ctx.Tools.Gradle = TestContexts.Tool(tmp.Root, ToolSource.Custom);
                Assert.IsEmpty(GradleRule.Evaluate(ctx));
            }
        }

        [Test] public void Env007_EmbeddedGradle_IsNotJudged()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("lib/gradle-launcher-8.4.jar");
                var ctx = TestContexts.Create();
                ctx.Tools.Gradle = TestContexts.Tool(tmp.Root, ToolSource.Embedded);
                Assert.IsEmpty(GradleRule.Evaluate(ctx));
            }
        }

        [Test] public void Env007_ExpectedVersionUnknown_IsSkipped()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("lib/gradle-launcher-8.4.jar");
                var ctx = TestContexts.Create(@"{ ""unity"": [ { ""match"": ""6000"", ""gradle"": null } ] }");
                ctx.Tools.Gradle = TestContexts.Tool(tmp.Root, ToolSource.Custom);
                Assert.IsEmpty(GradleRule.Evaluate(ctx));
            }
        }

        [Test] public void Env007_MissingCustomFolder_IsWarning()
        {
            var ctx = TestContexts.Create();
            ctx.Tools.Gradle = TestContexts.Tool("Z:/does/not/exist/gradle", ToolSource.Custom);
            Assert.AreEqual(Severity.Warning, GradleRule.Evaluate(ctx).Single().Severity);
        }
    }
}
