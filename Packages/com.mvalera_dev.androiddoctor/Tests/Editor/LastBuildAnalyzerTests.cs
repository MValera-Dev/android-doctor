using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using AndroidDoctor.Gradle;

namespace AndroidDoctor.Tests
{
    public class LastBuildAnalyzerTests
    {
        // --- where the log is ---

        [Test] public void EditorLogPath_IsAFileThatExists()
        {
            Assert.IsTrue(File.Exists(LogSource.EditorLogPath()));
        }

        [Test] public void EditorLogPath_FollowsWhatUnityReports()
        {
            // This Editor may write its log somewhere other than the platform default.
            if (!string.IsNullOrEmpty(Application.consoleLogPath) && File.Exists(Application.consoleLogPath))
                Assert.AreEqual(Application.consoleLogPath, LogSource.EditorLogPath());
        }

        [Test] public void ReadTail_ReadsOnlyTheEndOfABigFile()
        {
            using (var tmp = new TempDirectory())
            {
                string path = tmp.File("big.log", new string('a', 3000) + "THE-END");
                string tail = LogSource.ReadTail(path, 100);
                Assert.AreEqual(100, tail.Length);
                StringAssert.EndsWith("THE-END", tail);
            }
        }

        [Test] public void ReadTail_ReadsAFileThatIsStillOpenForWriting()
        {
            using (var tmp = new TempDirectory())
            {
                string path = Path.Combine(tmp.Root, "open.log");
                using (var writer = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes("written while open");
                    writer.Write(bytes, 0, bytes.Length);
                    writer.Flush();
                    Assert.AreEqual("written while open", LogSource.ReadTail(path));
                }
            }
        }

        [Test] public void FindGradleLogPath_TakesTheMostRecentOne()
        {
            string text = "Build log was saved at 'C:\\old\\a.log'\nstuff\nBuild log was saved at 'C:\\new\\b.log'\n";
            Assert.AreEqual("C:\\new\\b.log", LogSource.FindGradleLogPath(text));
        }

        [Test] public void FindGradleLogPath_NoMention_ReturnsNull()
        {
            Assert.IsNull(LogSource.FindGradleLogPath("nothing here"));
            Assert.IsNull(LogSource.FindGradleLogPath(null));
        }

        [Test] public void PlatformDefaultEditorLogPath_EndsInEditorLog()
        {
            StringAssert.EndsWith("Editor.log", LogSource.PlatformDefaultEditorLogPath());
        }

        // --- analysing the last build ---

        [Test] public void Analyze_FindsTheFailureInTheEditorLog()
        {
            var error = LastBuildAnalyzer.Analyze(GradleFixtures.Read("EDITORLOG_GRD014_1"), path => null);
            Assert.IsNotNull(error);
            StringAssert.Contains("Could not compile build file", error.WhatWentWrong);
        }

        [Test] public void Analyze_WhenTheEditorLogIsCutOff_FallsBackToTheGradleLogUnitySaved()
        {
            string editorLog = "Build log was saved at 'C:\\gradle\\unity-assembleRelease-build.log'\n(the failure itself was cut off)\n";
            string requested = null;

            var error = LastBuildAnalyzer.Analyze(editorLog, path => { requested = path; return GradleFixtures.Read("GRD006_1"); });

            Assert.AreEqual("C:\\gradle\\unity-assembleRelease-build.log", requested);
            StringAssert.Contains("Duplicate class", error.WhatWentWrong);
        }

        [Test] public void Analyze_NoFailureAnywhere_ReturnsNull()
        {
            Assert.IsNull(LastBuildAnalyzer.Analyze("an ordinary log\n", path => null));
            Assert.IsNull(LastBuildAnalyzer.Analyze("Build log was saved at 'C:\\x.log'\n", path => GradleFixtures.Read("CLEAN_1")));
        }

        [Test] public void Analyze_AGradleLogThatCannotBeRead_ReturnsNull()
        {
            Assert.IsNull(LastBuildAnalyzer.Analyze("Build log was saved at 'C:\\missing.log'\n", path => null));
        }
    }

    public class GradleErrorCaptureTests
    {
        string _original;

        [SetUp] public void SetUp() { _original = SessionState.GetString(GradleErrorCapture.SessionKey, ""); GradleErrorCapture.Clear(); }
        [TearDown] public void TearDown() { SessionState.SetString(GradleErrorCapture.SessionKey, _original); }

        [Test] public void AGradleErrorOnTheConsole_IsKept_AndExplainedWithAWarning()
        {
            string text = GradleFixtures.Read("GRD006_1");
            LogAssert.Expect(LogType.Warning, new Regex("Android Doctor: .*GRD006"));

            GradleErrorCapture.OnLog(text, "stack", LogType.Error);

            var kept = GradleErrorCapture.Get();
            Assert.IsNotNull(kept);
            StringAssert.Contains("Duplicate class", kept.WhatWentWrong);
        }

        [Test] public void AnUnrecognisedFailure_WarnsThatItIsNotRecognised()
        {
            LogAssert.Expect(LogType.Warning, new Regex("not recognised|no reconozco"));
            GradleErrorCapture.OnLog(GradleFixtures.Read("UNKNOWN_1"), "", LogType.Error);
            Assert.IsNotNull(GradleErrorCapture.Get());
        }

        [Test] public void OtherLogTypes_AreIgnored()
        {
            string text = GradleFixtures.Read("GRD006_1");
            GradleErrorCapture.OnLog(text, "", LogType.Warning);
            GradleErrorCapture.OnLog(text, "", LogType.Log);
            Assert.IsNull(GradleErrorCapture.Get());
        }

        [Test] public void AnErrorThatIsNotAboutGradle_IsIgnored()
        {
            GradleErrorCapture.OnLog("NullReferenceException: object reference not set", "at Foo.Bar", LogType.Exception);
            Assert.IsNull(GradleErrorCapture.Get());
        }

        [Test] public void AnErrorThatOnlyHasTheGradleStderr_IsIgnoredUntilTheFailureBlockArrives()
        {
            // Unity logs the failure twice: first the stderr part, then the FAILURE block.
            GradleErrorCapture.OnLog("[Incubating] Problems report is available at: file:///x", "", LogType.Error);
            Assert.IsNull(GradleErrorCapture.Get());
        }

        [Test] public void Clear_ForgetsTheCapturedError()
        {
            LogAssert.Expect(LogType.Warning, new Regex("Android Doctor"));
            GradleErrorCapture.OnLog(GradleFixtures.Read("GRD006_1"), "", LogType.Error);
            GradleErrorCapture.Clear();
            Assert.IsNull(GradleErrorCapture.Get());
        }
    }

    public class ForumReportTests
    {
        [Test] public void Report_HasTheVersions_AndTheOriginalBlockInsideFences()
        {
            var error = GradleLogParser.Extract(GradleFixtures.Read("GRD006_1"));

            string text = ForumReport.Build(error, "6000.6.4f1", "Windows 11");

            StringAssert.StartsWith("Unity 6000.6.4f1 · Windows 11", text);
            StringAssert.Contains("```", text);
            StringAssert.Contains("Duplicate class com.example.Dup", text);
        }

        [Test] public void Report_WithoutAnError_StillHasTheVersions()
        {
            StringAssert.Contains("6000.6.4f1", ForumReport.Build(null, "6000.6.4f1", "Windows 11"));
        }
    }
}
