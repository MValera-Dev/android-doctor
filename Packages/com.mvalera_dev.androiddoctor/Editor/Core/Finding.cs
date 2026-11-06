using System.Collections.Generic;

namespace AndroidDoctor
{
    /// <summary>The result of any rule. Immutable.</summary>
    public sealed class Finding
    {
        public readonly string RuleId;   // "ENV004"
        public readonly Category Category;
        public readonly Severity Severity;
        public readonly string Title;    // one line
        public readonly string Detail;   // one or two sentences: what happens and why it matters
        public readonly IReadOnlyList<string> Evidence; // paths, versions; one per line
        public readonly string FixId;    // null when there is no automatic fix
        public readonly string FixLabel; // button text
        public readonly string Command;  // console command that can be copied (optional)
        public readonly string DocsUrl;  // optional

        public Finding(string ruleId, Category category, Severity severity, string title, string detail,
            IReadOnlyList<string> evidence = null, string fixId = null, string fixLabel = null,
            string command = null, string docsUrl = null)
        {
            RuleId = ruleId; Category = category; Severity = severity; Title = title; Detail = detail;
            Evidence = evidence ?? new string[0];
            FixId = fixId; FixLabel = fixLabel; Command = command; DocsUrl = docsUrl;
        }
    }
}
