using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace AndroidDoctor.Tests
{
    public class ReportBuilderTests
    {
        static ReportEnvironment Env() => new ReportEnvironment
        {
            PackageVersion = "1.0.0", UnityVersion = "6000.3.2f1", OperatingSystem = "Windows 11", TargetApi = "36",
            Backend = "IL2CPP", Architectures = "ARM64", Jdk = "17.0.18 (embedded)", Ndk = "27.2.12479018 (embedded)",
        };

        static Finding F(string id, Severity severity, string title, string detail, string[] evidence = null, string command = null)
        {
            return new Finding(id, Category.Project, severity, title, detail, evidence, command: command);
        }

        static List<Finding> FixedFindings()
        {
            return new List<Finding>
            {
                F("NAT001", Severity.Error, "Native libraries from Packages/com.vendor.sdk are not 16 KB aligned (4)",
                    "Update the package or ask the vendor for a version with 16 KB support.", new[]
                    {
                        "libfoo.so (arm64-v8a) · LOAD align 0x1000 · Packages/com.vendor.sdk/Plugins/arm64-v8a/libfoo.so",
                        "libbar.so (arm64-v8a) · LOAD align 0x1000 · Packages/com.vendor.sdk/Plugins/arm64-v8a/libbar.so",
                        "libbaz.so (x86_64) · LOAD align 0x1000 · Packages/com.vendor.sdk/Plugins/x86_64/libbaz.so",
                        "libqux.so (x86_64) · LOAD align 0x1000 · Packages/com.vendor.sdk/Plugins/x86_64/libqux.so",
                    }),
                F("ENV004", Severity.Error, "SDK platform android-36 is not installed",
                    "Your target API level is 36, but the SDK has no platform for it, so the build will fail.",
                    new[] { "SDK path: C:/sdk" }, "sdkmanager \"platforms;android-36\""),
                F("PRJ005", Severity.Warning, "Custom Gradle templates found",
                    "Custom templates replace Unity's defaults and can break after an upgrade.",
                    new[] { "Assets/Plugins/Android/mainTemplate.gradle" }),
                F("PRJ004", Severity.Info, "Android App Bundle is off",
                    "Google Play requires an Android App Bundle (.aab) for new apps.", new[] { "Build App Bundle: off" }),
            };
        }

        static string Normalize(string s) => s.Replace("\r\n", "\n");

        static string Golden()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TempDirectory).Assembly);
            return File.ReadAllText(Path.Combine(package.resolvedPath, "Tests", "Editor", "Fixtures", "report_golden.md"));
        }

        // The golden file pins the format. If the format changes on purpose, regenerate the file in the same commit.
        [Test] public void Report_MatchesTheGoldenFile()
        {
            Assert.AreEqual(Normalize(Golden()), Normalize(ReportBuilder.Build(Env(), FixedFindings())));
        }

        [Test] public void Header_HasTheVersionsAndUsesUnknownForWhatIsMissing()
        {
            var lines = ReportBuilder.Build(Env(), new List<Finding>()).Split('\n');
            Assert.AreEqual("## Android Doctor 1.0.0", lines[0]);
            Assert.AreEqual("Unity 6000.3.2f1 · Windows 11 · target API 36 · IL2CPP · ARM64", lines[1]);
            Assert.AreEqual("JDK 17.0.18 (embedded) · NDK 27.2.12479018 (embedded) · Gradle unknown", lines[2]);
        }

        [Test] public void NoFindings_SaysSo()
        {
            StringAssert.Contains("No findings.", ReportBuilder.Build(Env(), new List<Finding>()));
            StringAssert.Contains("No findings.", ReportBuilder.Build(Env(), null));
        }

        [Test] public void EmptySeverities_AreLeftOut()
        {
            string report = ReportBuilder.Build(Env(), new List<Finding> { F("PRJ004", Severity.Info, "t", "d") });
            StringAssert.DoesNotContain("### Errors", report);
            StringAssert.DoesNotContain("### Warnings", report);
            StringAssert.Contains("### Info (1)", report);
        }

        [Test] public void OnlyTheFirstThreeEvidenceLinesAreShown()
        {
            string report = ReportBuilder.Build(Env(), FixedFindings());
            StringAssert.Contains("libbaz.so", report);
            StringAssert.DoesNotContain("libqux.so", report);
        }

        [Test] public void ATitleOrDetailEndsWithExactlyOneFullStop()
        {
            string report = ReportBuilder.Build(Env(), new List<Finding> { F("X001", Severity.Error, "Title with a stop.", "Detail without one") });
            StringAssert.Contains("- **X001** Title with a stop. Detail without one.", report);
            StringAssert.DoesNotContain("..", report);
        }

        [Test] public void MultiLineTextsAreCollapsedToOneLine()
        {
            string report = ReportBuilder.Build(Env(), new List<Finding> { F("X001", Severity.Error, "Two\nlines", "and\r\nmore", new[] { "ev\nidence" }) });
            StringAssert.Contains("- **X001** Two lines. and  more.", report);
            StringAssert.Contains("  - ev idence", report);
        }

        [Test] public void ALongReport_IsCutAtTheLimitWithAMarker()
        {
            var many = Enumerable.Range(0, 400).Select(i => F("X" + i.ToString("000"), Severity.Warning, "A finding with a reasonably long title number " + i, "and a detail sentence")).ToList();

            string report = ReportBuilder.Build(Env(), many);

            Assert.AreEqual(ReportBuilder.MaxLength, report.Length);
            StringAssert.EndsWith("… (truncated)", report);
        }

        [Test] public void AShortReport_IsNotMarkedTruncated()
        {
            StringAssert.DoesNotContain("truncated", ReportBuilder.Build(Env(), FixedFindings()));
        }

        [Test] public void TheReportIsLanguageIndependent_BecauseItOnlyUsesWhatItIsGiven()
        {
            // Headers and section names are fixed English; only the findings' own texts vary with the UI language.
            string report = ReportBuilder.Build(Env(), FixedFindings());
            StringAssert.Contains("### Errors (2)", report);
            StringAssert.Contains("### Warnings (1)", report);
        }
    }
}
