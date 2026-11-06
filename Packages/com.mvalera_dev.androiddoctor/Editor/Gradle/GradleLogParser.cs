using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AndroidDoctor.Gradle
{
    /// <summary>The part of a Gradle log that describes a failed build.</summary>
    public sealed class GradleError
    {
        public string Block;          // from the FAILURE line to BUILD FAILED, as Gradle printed it
        public string WhatWentWrong;  // the "* What went wrong:" texts, one per failure, separated by a blank line
    }

    public static class GradleLogParser
    {
        // Gradle 9 prints "FAILURE: Build completed with 2 failures." when several tasks failed and
        // "FAILURE: Build failed with an exception." when one did. Both start the failure report.
        static readonly Regex FailureLine = new Regex(
            @"^FAILURE: Build (?:failed with an exception|completed with \d+ failures?)", RegexOptions.Multiline);
        static readonly Regex BuildFailedLine = new Regex(@"^BUILD FAILED[^\r\n]*", RegexOptions.Multiline);
        static readonly Regex WhatWentWrong = new Regex(
            @"\* What went wrong:\r?\n(.*?)\r?\n\s*\* (?:Try|Exception is):", RegexOptions.Singleline);
        const string LegacyMarker = "CommandInvokationFailure: Gradle build failed";
        const int LegacyBlockLines = 60;

        /// <summary>The last failed build in the text, or null if there is none.</summary>
        public static GradleError Extract(string log)
        {
            if (string.IsNullOrEmpty(log)) return null;

            int start = LastIndexOfFailure(log);
            if (start >= 0)
            {
                var end = BuildFailedLine.Match(log, start);
                int stop = end.Success ? end.Index + end.Length : log.Length;
                string block = log.Substring(start, stop - start);
                return new GradleError { Block = block, WhatWentWrong = ReadWhatWentWrong(block) };
            }

            // Older Unity versions report the failure with this marker instead.
            int legacy = log.LastIndexOf(LegacyMarker, System.StringComparison.Ordinal);
            if (legacy >= 0)
            {
                string[] lines = log.Substring(legacy).Split('\n');
                var take = new List<string>();
                for (int i = 0; i < lines.Length && i < LegacyBlockLines; i++) take.Add(lines[i].TrimEnd('\r'));
                string block = string.Join("\n", take);
                return new GradleError { Block = block, WhatWentWrong = ReadWhatWentWrong(block) };
            }
            return null;
        }

        static int LastIndexOfFailure(string log)
        {
            int index = -1;
            foreach (Match m in FailureLine.Matches(log)) index = m.Index;
            return index;
        }

        static string ReadWhatWentWrong(string block)
        {
            var parts = new List<string>();
            foreach (Match m in WhatWentWrong.Matches(block)) parts.Add(m.Groups[1].Value.Trim());
            return string.Join("\n\n", parts);
        }
    }
}
