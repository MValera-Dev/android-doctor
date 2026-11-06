using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Native;

namespace AndroidDoctor.Tests
{
    /// <summary>NAT004 (Burst) and NAT005 (App Bundle package), driven by play.json data.</summary>
    public class KnownPackageRulesTests
    {
        const string PlayJson = @"{ ""knownPackages"": [
            { ""name"": ""com.unity.burst"", ""minVersion"": ""1.8.21"", ""severity"": ""Error"", ""reason"": ""burst reason"" },
            { ""name"": ""com.google.android.appbundle"", ""minVersion"": ""1.10.0"", ""severity"": ""Warning"", ""reason"": ""appbundle reason"" } ] }";

        static RuleContext Context(string package, string version)
        {
            var ctx = TestContexts.Create();
            ctx.Play = PlayRequirements.Parse(PlayJson);
            if (package != null) ctx.Packages.Add(new PackageRef { Name = package, Version = version, ResolvedPath = "x" });
            return ctx;
        }

        [Test] public void Nat004_OldBurst_IsError_WithVersionsAndReason()
        {
            var f = new BurstVersionRule().Evaluate(Context("com.unity.burst", "1.8.20")).Single();
            Assert.AreEqual("NAT004", f.RuleId);
            Assert.AreEqual(Severity.Error, f.Severity);
            Assert.AreEqual(Category.Native, f.Category);
            StringAssert.Contains("1.8.20", f.Title);
            StringAssert.Contains("1.8.21", f.Title);
            Assert.IsTrue(f.Evidence.Any(e => e.Contains("burst reason")));
        }

        [TestCase("1.8.21")]
        [TestCase("1.9.0")]
        [TestCase("2.0.0")]
        public void Nat004_BurstAtOrAboveTheMinimum_NoFinding(string version)
        {
            Assert.IsEmpty(new BurstVersionRule().Evaluate(Context("com.unity.burst", version)));
        }

        [Test] public void Nat004_PrereleaseOfTheMinimum_IsNotReportedAsOlder()
        {
            Assert.IsEmpty(new BurstVersionRule().Evaluate(Context("com.unity.burst", "1.8.21-pre.1")));
        }

        [Test] public void Nat004_BurstNotInstalled_NoFinding()
        {
            Assert.IsEmpty(new BurstVersionRule().Evaluate(Context(null, null)));
        }

        [Test] public void Nat004_UnparseableInstalledVersion_NoFinding()
        {
            Assert.IsEmpty(new BurstVersionRule().Evaluate(Context("com.unity.burst", "custom-build")));
        }

        [Test] public void Nat004_NoPlayData_NoFinding()
        {
            var ctx = Context("com.unity.burst", "1.0.0");
            ctx.Play = null;
            Assert.IsEmpty(new BurstVersionRule().Evaluate(ctx));
        }

        [Test] public void Nat005_OldAppBundlePackage_IsWarning_FromTheData()
        {
            var f = new AppBundlePackageRule().Evaluate(Context("com.google.android.appbundle", "1.9.0")).Single();
            Assert.AreEqual("NAT005", f.RuleId);
            Assert.AreEqual(Severity.Warning, f.Severity);
        }

        [Test] public void Nat005_RuleOnlyLooksAtItsOwnPackage()
        {
            Assert.IsEmpty(new AppBundlePackageRule().Evaluate(Context("com.unity.burst", "1.0.0")));
            Assert.IsEmpty(new BurstVersionRule().Evaluate(Context("com.google.android.appbundle", "1.0.0")));
        }

        [Test] public void SeverityComesFromTheData_NotFromTheRule()
        {
            var ctx = Context("com.unity.burst", "1.0.0");
            ctx.Play = PlayRequirements.Parse(@"{ ""knownPackages"": [ { ""name"": ""com.unity.burst"", ""minVersion"": ""1.8.21"", ""severity"": ""Info"" } ] }");
            Assert.AreEqual(Severity.Info, new BurstVersionRule().Evaluate(ctx).Single().Severity);

            ctx.Play = PlayRequirements.Parse(@"{ ""knownPackages"": [ { ""name"": ""com.unity.burst"", ""minVersion"": ""1.8.21"", ""severity"": ""nonsense"" } ] }");
            Assert.AreEqual(Severity.Warning, new BurstVersionRule().Evaluate(ctx).Single().Severity);
        }

        [Test] public void TheShippedPlayJson_KnowsBothPackages()
        {
            var play = PlayRequirements.Load();
            Assert.IsNotNull(play);
            Assert.IsTrue(play.knownPackages.Any(k => k.name == "com.unity.burst"));
            Assert.IsTrue(play.knownPackages.Any(k => k.name == "com.google.android.appbundle"));
        }
    }
}
