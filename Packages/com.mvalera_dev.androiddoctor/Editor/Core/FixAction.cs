namespace AndroidDoctor
{
    /// <summary>
    /// One thing the user can press on a finding. A finding can offer several, for example one per version of a
    /// duplicated library. Immutable.
    /// </summary>
    public sealed class FixAction
    {
        public readonly string FixId;   // the IFix that does the work
        public readonly string Label;   // button text
        public readonly string Arg;     // what the fix needs to know (a tool, a path, a version); may be null
        /// <summary>
        /// True when this action needs no decision from the user and is low risk, so "Fix all" may include it.
        /// A choice between options (which library to keep) is never part of "Fix all".
        /// </summary>
        public readonly bool AllInOne;

        public FixAction(string fixId, string label, string arg = null, bool allInOne = false)
        {
            FixId = fixId; Label = label; Arg = arg; AllInOne = allInOne;
        }
    }
}
