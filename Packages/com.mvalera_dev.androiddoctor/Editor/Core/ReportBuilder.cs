using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndroidDoctor
{
    /// <summary>What the report header says about the machine and the project. Plain strings, filled by the caller.</summary>
    public sealed class ReportEnvironment
    {
        public string PackageVersion = "dev";
        public string UnityVersion = "unknown";
        public string OperatingSystem = "unknown";
        public string TargetApi = "unknown";      // "36" or "Auto"
        public string Backend = "unknown";        // "IL2CPP"
        public string Architectures = "unknown";  // "ARM64"
        public string Jdk = "unknown";            // "17.0.18 (embedded)"
        public string Ndk = "unknown";
        public string Gradle = "unknown";
    }

    /// <summary>The Markdown report that is pasted into a forum or an issue. English on purpose, so anyone can read it.</summary>
    public static class ReportBuilder
    {
        public const int MaxLength = 8000;
        const string Truncated = "… (truncated)";
        const int MaxEvidenceLines = 3;

        public static string Build(ReportEnvironment env, IList<Finding> findings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Android Doctor " + env.PackageVersion);
            sb.AppendLine("Unity " + env.UnityVersion + " · " + env.OperatingSystem + " · target API " + env.TargetApi
                + " · " + env.Backend + " · " + env.Architectures);
            sb.AppendLine("JDK " + env.Jdk + " · NDK " + env.Ndk + " · Gradle " + env.Gradle);

            if (findings == null || findings.Count == 0)
            {
                sb.AppendLine();
                sb.AppendLine("No findings.");
            }
            else
            {
                AppendSection(sb, "Errors", findings.Where(f => f.Severity == Severity.Error).ToList());
                AppendSection(sb, "Warnings", findings.Where(f => f.Severity == Severity.Warning).ToList());
                AppendSection(sb, "Info", findings.Where(f => f.Severity == Severity.Info).ToList());
            }

            string text = sb.ToString().Replace("\r\n", "\n").TrimEnd('\n') + "\n";
            return text.Length <= MaxLength ? text : text.Substring(0, MaxLength - Truncated.Length) + Truncated;
        }

        static void AppendSection(StringBuilder sb, string name, List<Finding> items)
        {
            if (items.Count == 0) return;
            sb.AppendLine();
            sb.AppendLine("### " + name + " (" + items.Count + ")");
            foreach (var f in items)
            {
                sb.AppendLine("- **" + f.RuleId + "** " + Sentence(f.Title) + (string.IsNullOrEmpty(f.Detail) ? "" : " " + Sentence(f.Detail)));
                foreach (string line in f.Evidence.Take(MaxEvidenceLines)) sb.AppendLine("  - " + OneLine(line));
                if (!string.IsNullOrEmpty(f.Command)) sb.AppendLine("  - Run: `" + OneLine(f.Command) + "`");
            }
        }

        // A title or detail ends with exactly one full stop, whether or not the original text had one.
        static string Sentence(string text)
        {
            string t = OneLine(text).TrimEnd();
            if (t.Length == 0) return "";
            return t.TrimEnd('.') + ".";
        }

        static string OneLine(string text) => (text ?? "").Replace("\r", " ").Replace("\n", " ");
    }
}
