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
}
