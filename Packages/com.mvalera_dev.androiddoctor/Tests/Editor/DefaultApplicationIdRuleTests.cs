using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using AndroidDoctor.Project;

namespace AndroidDoctor.Tests
{
    public class DefaultApplicationIdRuleTests
    {
        string _original;

        [SetUp] public void SetUp()
        {
            _original = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
        }

        [TearDown] public void TearDown()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, _original);
        }

        static Finding[] Run() => new DefaultApplicationIdRule().Run(new RuleContext()).ToArray();

        [Test] public void DefaultCompanyId_ProducesPrj009Warning()
        {
            // Unity strips characters that are invalid in an Android id (a hyphen, for example),
            // so use an id that is already valid and read back what Unity actually stored.
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.DefaultCompany.androiddoctor");
            string stored = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);

            var findings = Run();

            Assert.AreEqual(1, findings.Length);
            Assert.AreEqual("PRJ009", findings[0].RuleId);
            Assert.AreEqual(Severity.Warning, findings[0].Severity);
            Assert.AreEqual(Category.Project, findings[0].Category);
            StringAssert.Contains(stored, findings[0].Evidence[0]);
        }

        [Test] public void CustomId_ProducesNoFinding()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.mvalera_dev.androiddoctortest");

            Assert.IsEmpty(Run());
        }
    }
}
