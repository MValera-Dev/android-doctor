using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidDoctor.Project
{
    /// <summary>PRJ007: the same .aar/.jar artifact present in more than one version inside Assets/.</summary>
    public sealed class DuplicateLibrariesRule : IRule
    {
        public string Id => "PRJ007";
        public Category Category => Category.Project;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx);

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx)
        {
            if (string.IsNullOrEmpty(ctx.AssetsPath)) yield break;

            var files = ProjectFileWalker.FindFiles(ctx.AssetsPath, file =>
            {
                string a, v;
                return ArtifactName.TryParse(file, out a, out v);
            });

            var groups = new Dictionary<string, List<KeyValuePair<string, string>>>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in files)
            {
                string artifact, version;
                ArtifactName.TryParse(file, out artifact, out version);
                List<KeyValuePair<string, string>> list;
                if (!groups.TryGetValue(artifact, out list))
                    groups[artifact] = list = new List<KeyValuePair<string, string>>();
                list.Add(new KeyValuePair<string, string>(ProjectFileWalker.ToProjectRelative(ctx.ProjectPath, file), version));
            }

            foreach (var group in groups.OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
            {
                var versions = group.Value.Where(e => e.Value != null).Select(e => e.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v, StringComparer.Ordinal).ToList();
                if (versions.Count < 2) continue; // same version twice, or no version at all: not this rule

                var paths = group.Value.Select(e => e.Key).OrderBy(p => p, StringComparer.Ordinal).ToList();
                yield return new Finding("PRJ007", Category.Project, Severity.Warning,
                    Messages.Get("PRJ007.title", group.Key), Messages.Get("PRJ007.detail", string.Join(", ", versions)),
                    paths, FixIds.SelectAsset, Messages.Get("PRJ007.fix"));
            }
        }
    }
}
