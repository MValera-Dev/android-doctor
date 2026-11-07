using System.Collections.Generic;

namespace AndroidDoctor
{
    public interface IRule
    {
        string Id { get; }             // stable forever
        Category Category { get; }
        IEnumerable<Finding> Run(RuleContext ctx);
    }

    public interface IFix
    {
        string Id { get; }
        string Describe(Finding finding);                 // "before -> after" text for the dialog
        bool Apply(Finding finding, out string message); // true if applied
    }

    /// <summary>A fix that needs to know which action was pressed, because the finding offers several or passes an argument.</summary>
    public interface IActionFix : IFix
    {
        string Describe(Finding finding, FixAction action);
        bool Apply(Finding finding, FixAction action, out string message);
    }
}
