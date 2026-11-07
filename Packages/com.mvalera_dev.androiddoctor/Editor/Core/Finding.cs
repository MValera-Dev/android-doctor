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
        public readonly IReadOnlyList<FixAction> Actions; // what the user can press; empty when nothing can be done for them
        public readonly string Command;  // console command that can be copied (optional)
        public readonly string DocsUrl;  // optional

        /// <summary>The id of the first action, or null. Kept so a finding with one fix reads like it always did.</summary>
        public string FixId => Actions.Count > 0 ? Actions[0].FixId : null;
        /// <summary>The label of the first action, or null.</summary>
        public string FixLabel => Actions.Count > 0 ? Actions[0].Label : null;

        public Finding(string ruleId, Category category, Severity severity, string title, string detail,
            IReadOnlyList<string> evidence = null, string fixId = null, string fixLabel = null,
            string command = null, string docsUrl = null, IReadOnlyList<FixAction> actions = null)
        {
            RuleId = ruleId; Category = category; Severity = severity; Title = title; Detail = detail;
            Evidence = evidence ?? new string[0];
            Command = command; DocsUrl = docsUrl;

            if (actions != null) Actions = actions;
            else if (!string.IsNullOrEmpty(fixId)) Actions = new[] { new FixAction(fixId, fixLabel, null, FixIds.ChangesSettings(fixId)) };
            else Actions = new FixAction[0];
        }
    }
}
