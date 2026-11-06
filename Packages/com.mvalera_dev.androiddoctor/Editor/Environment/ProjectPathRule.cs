using System.Collections.Generic;
using UnityEngine;

namespace AndroidDoctor.Environment
{
    /// <summary>ENV009: project paths that tend to break Android builds. A heuristic, with no official source.</summary>
    public sealed class ProjectPathRule : IRule
    {
        internal const int MaxWindowsPathLength = 200;

        public string Id => "ENV009";
        public Category Category => Category.Environment;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            return Evaluate(ctx.ProjectPath, Application.platform == RuntimePlatform.WindowsEditor);
        }

        internal static IEnumerable<Finding> Evaluate(string projectPath, bool isWindows)
        {
            if (string.IsNullOrEmpty(projectPath)) yield break;

            if (isWindows && projectPath.Length > MaxWindowsPathLength)
            {
                yield return new Finding("ENV009", Category.Environment, Severity.Warning,
                    Messages.Get("ENV009.long.title"), Messages.Get("ENV009.long.detail"),
                    new[] { Messages.Get("ENV009.long.evidence", projectPath.Length, projectPath) });
            }

            if (HasNonAscii(projectPath))
            {
                yield return new Finding("ENV009", Category.Environment, Severity.Info,
                    Messages.Get("ENV009.ascii.title"), Messages.Get("ENV009.ascii.detail"),
                    new[] { Messages.Get("ENV009.ascii.evidence", projectPath) });
            }
        }

        static bool HasNonAscii(string s)
        {
            foreach (char c in s) if (c > 127) return true;
            return false;
        }
    }
}
