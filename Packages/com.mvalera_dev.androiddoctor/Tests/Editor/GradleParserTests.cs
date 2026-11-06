using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using AndroidDoctor.Gradle;

namespace AndroidDoctor.Tests
{
    public static class GradleFixtures
    {
        public static string Dir()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TempDirectory).Assembly);
            return Path.Combine(package.resolvedPath, "Tests", "Editor", "Fixtures", "Gradle");
        }

        public static string Read(string name) => File.ReadAllText(Path.Combine(Dir(), name + ".txt"));

        /// <summary>Names of the real failed-build logs: GRD003_1, GRD004_1... UNKNOWN_1. Not CLEAN or EDITORLOG.</summary>
        public static string[] FailureNames()
        {
            return Directory.GetFiles(Dir(), "*.txt")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(n => !n.StartsWith("CLEAN_") && !n.StartsWith("EDITORLOG_"))
                .OrderBy(n => n).ToArray();
        }
    }

    public class GradleParserTests
    {
        [Test] public void Extract_FromARealFailedBuild_FindsTheBlockAndTheCauses()
        {
            var e = GradleLogParser.Extract(GradleFixtures.Read("GRD014_1"));

            Assert.IsNotNull(e);
            StringAssert.StartsWith("FAILURE: Build completed with 2 failures", e.Block);
            StringAssert.Contains("BUILD FAILED", e.Block);
            // Two failures, two "What went wrong" texts.
            StringAssert.Contains("Could not compile build file", e.WhatWentWrong);
            StringAssert.Contains("does not specify `compileSdk`", e.WhatWentWrong);
        }

        [Test] public void Extract_ASingleFailure_IsReadToo()
        {
            string log = "noise\nFAILURE: Build failed with an exception.\n\n* What went wrong:\nSomething broke.\n> detail\n\n* Try:\n> Run with --stacktrace\n\nBUILD FAILED in 3s\nafter\n";
            var e = GradleLogParser.Extract(log);
            Assert.AreEqual("Something broke.\n> detail", e.WhatWentWrong);
            StringAssert.EndsWith("BUILD FAILED in 3s", e.Block);
            StringAssert.DoesNotContain("after", e.Block);
            StringAssert.DoesNotContain("noise", e.Block);
        }

        [Test] public void Extract_ACleanBuild_ReturnsNull()
        {
            Assert.IsNull(GradleLogParser.Extract(GradleFixtures.Read("CLEAN_1")));
        }

        [Test] public void Extract_NullOrEmptyOrUnrelatedText_ReturnsNull()
        {
            Assert.IsNull(GradleLogParser.Extract(null));
            Assert.IsNull(GradleLogParser.Extract(""));
            Assert.IsNull(GradleLogParser.Extract("just some Unity log\nwith lines\n"));
        }

        [Test] public void Extract_UsesTheLastFailureInTheText()
        {
            string first = "FAILURE: Build failed with an exception.\n\n* What went wrong:\nOld problem.\n\n* Try:\n> x\n\nBUILD FAILED in 1s\n";
            string second = "FAILURE: Build failed with an exception.\n\n* What went wrong:\nNew problem.\n\n* Try:\n> x\n\nBUILD FAILED in 2s\n";
            Assert.AreEqual("New problem.", GradleLogParser.Extract(first + "in between\n" + second).WhatWentWrong);
        }

        [Test] public void Extract_FailureWithoutBuildFailedLine_RunsToTheEndOfTheText()
        {
            var e = GradleLogParser.Extract("FAILURE: Build failed with an exception.\n\n* What went wrong:\nCut off.\n\n* Try:\n> x\n");
            StringAssert.Contains("Cut off.", e.Block);
            Assert.AreEqual("Cut off.", e.WhatWentWrong);
        }

        [Test] public void Extract_LegacyMarker_TakesTheNextSixtyLines()
        {
            var sb = new StringBuilder("CommandInvokationFailure: Gradle build failed.\n");
            for (int i = 1; i <= 100; i++) sb.Append("line ").Append(i).Append('\n');

            var e = GradleLogParser.Extract(sb.ToString());

            Assert.IsNotNull(e);
            Assert.AreEqual(60, e.Block.Split('\n').Length);
            StringAssert.Contains("line 59", e.Block);
            StringAssert.DoesNotContain("line 60\n", e.Block + "\n");
        }

        [Test] public void Extract_FromARealEditorLogSlice_FindsTheSameFailure()
        {
            var e = GradleLogParser.Extract(GradleFixtures.Read("EDITORLOG_GRD014_1"));
            Assert.IsNotNull(e);
            StringAssert.Contains("Could not compile build file", e.WhatWentWrong);
        }

        [Test] public void Extract_HandlesWindowsLineEndings()
        {
            string log = "FAILURE: Build failed with an exception.\r\n\r\n* What went wrong:\r\nBroke on Windows.\r\n\r\n* Try:\r\n> x\r\n\r\nBUILD FAILED in 1s\r\n";
            Assert.AreEqual("Broke on Windows.", GradleLogParser.Extract(log).WhatWentWrong);
        }
    }
}
