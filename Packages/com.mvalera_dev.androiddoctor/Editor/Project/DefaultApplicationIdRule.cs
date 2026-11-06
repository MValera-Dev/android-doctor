using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace AndroidDoctor.Project
{
    public sealed class DefaultApplicationIdRule : IRule
    {
        public string Id => "PRJ009";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            string id = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            if (string.IsNullOrEmpty(id) || id.StartsWith("com.DefaultCompany."))
            {
                yield return new Finding(Id, Category, Severity.Warning,
                    Messages.Get("PRJ009.title"), Messages.Get("PRJ009.detail"),
                    new[] { Messages.Get("PRJ009.evidence", id) });
            }
        }
    }
}
