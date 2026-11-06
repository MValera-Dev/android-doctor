using System;
using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Environment;

namespace AndroidDoctor.Tests
{
    /// <summary>ENV008 and ENV009: environment variables and the project path.</summary>
    public class SystemRulesTests
    {
        const string SdkHome = "ANDROID_SDK_HOME";
        string _originalSdkHome;

        [SetUp] public void SetUp()
        {
            _originalSdkHome = System.Environment.GetEnvironmentVariable(SdkHome);
        }

        [TearDown] public void TearDown()
        {
            System.Environment.SetEnvironmentVariable(SdkHome, _originalSdkHome);
        }

        // --- ENV008 ---

        static Func<string, string> Vars(params string[] namesAndValues)
        {
            return name =>
            {
                for (int i = 0; i + 1 < namesAndValues.Length; i += 2)
                    if (namesAndValues[i] == name) return namesAndValues[i + 1];
                return null;
            };
        }

        [Test] public void Env008_AndroidSdkHome_IsWarning()
        {
            var f = EnvironmentVariablesRule.Evaluate(Vars(SdkHome, "C:/sdk")).Single();
            Assert.AreEqual("ENV008", f.RuleId);
            Assert.AreEqual(Severity.Warning, f.Severity);
            StringAssert.Contains("ANDROID_SDK_HOME=C:/sdk", f.Evidence[0]);
        }

        [Test] public void Env008_OtherVariables_AreInfo()
        {
            var findings = EnvironmentVariablesRule.Evaluate(Vars(
                "ANDROID_HOME", "C:/a", "JAVA_HOME", "C:/j", "ANDROID_NDK_HOME", "C:/n", "GRADLE_USER_HOME", "C:/g")).ToArray();
            Assert.AreEqual(4, findings.Length);
            Assert.IsTrue(findings.All(f => f.Severity == Severity.Info));
        }

        [Test] public void Env008_NothingSet_NoFinding()
        {
            Assert.IsEmpty(EnvironmentVariablesRule.Evaluate(Vars()));
        }

        [Test] public void Env008_EmptyValue_IsIgnored()
        {
            Assert.IsEmpty(EnvironmentVariablesRule.Evaluate(Vars(SdkHome, "")));
        }

        [Test] public void Env008_ReadsTheRealProcessEnvironment()
        {
            System.Environment.SetEnvironmentVariable(SdkHome, "C:/real-process-value");
            var findings = new EnvironmentVariablesRule().Run(TestContexts.Create()).ToArray();
            Assert.IsTrue(findings.Any(f => f.Severity == Severity.Warning && f.Evidence[0].Contains("C:/real-process-value")));

            System.Environment.SetEnvironmentVariable(SdkHome, null);
            findings = new EnvironmentVariablesRule().Run(TestContexts.Create()).ToArray();
            Assert.IsFalse(findings.Any(f => f.Title.Contains(SdkHome)));
        }

        // --- ENV009 ---

        static string PathOfLength(int length) => "C:\\" + new string('a', length - 3);

        [Test] public void Env009_LongPathOnWindows_IsWarning()
        {
            var f = ProjectPathRule.Evaluate(PathOfLength(201), true).Single();
            Assert.AreEqual("ENV009", f.RuleId);
            Assert.AreEqual(Severity.Warning, f.Severity);
            StringAssert.Contains("201", f.Evidence[0]);
        }

        [Test] public void Env009_PathOfExactlyTheLimit_NoFinding()
        {
            Assert.IsEmpty(ProjectPathRule.Evaluate(PathOfLength(ProjectPathRule.MaxWindowsPathLength), true));
        }

        [Test] public void Env009_LongPathOutsideWindows_NoFinding()
        {
            Assert.IsEmpty(ProjectPathRule.Evaluate(PathOfLength(300), false));
        }

        [Test] public void Env009_NonAsciiPath_IsInfo()
        {
            var f = ProjectPathRule.Evaluate("C:\\Dev\\Pr\u00f3yectos\\Android Doctor", true).Single();
            Assert.AreEqual(Severity.Info, f.Severity);
        }

        [Test] public void Env009_PlainShortPath_NoFinding()
        {
            Assert.IsEmpty(ProjectPathRule.Evaluate("C:\\Dev\\Unity\\Projects\\Android Doctor", true));
        }

        [Test] public void Env009_LongAndNonAscii_ReportsBoth()
        {
            string path = "C:\\\u00f1" + new string('a', 250);
            Assert.AreEqual(2, ProjectPathRule.Evaluate(path, true).Count());
        }

        [Test] public void Env009_EmptyPath_IsSkipped()
        {
            Assert.IsEmpty(ProjectPathRule.Evaluate("", true));
            Assert.IsEmpty(ProjectPathRule.Evaluate(null, true));
        }
    }
}
