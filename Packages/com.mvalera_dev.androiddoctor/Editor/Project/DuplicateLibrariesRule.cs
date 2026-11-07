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

        /// <summary>One button per version ("keep this one, turn the others off"), newest first, and one to look at the files.</summary>
        internal static FixAction[] KeepOneVersion(List<KeyValuePair<string, string>> files, List<string> versions)
        {
            var ordered = versions.OrderBy(v => v, Comparer<string>.Create((a, b) => VersionCompare.IsLess(a, b) ? 1 : VersionCompare.IsLess(b, a) ? -1 : 0)).ToList();
            var actions = new List<FixAction>();
            foreach (string keep in ordered)
            {
                var others = files.Where(e => !string.Equals(e.Value, keep, StringComparison.OrdinalIgnoreCase)).Select(e => e.Key).ToList();
                string label = Messages.Get(actions.Count == 0 ? "PRJ007.keepNewest" : "PRJ007.keep", keep);
                actions.Add(new FixAction(FixIds.DisablePlugin, label, string.Join(DisablePluginFix.Separator.ToString(), others)));
            }
            actions.Add(new FixAction(FixIds.SelectAsset, Messages.Get("PRJ007.fix")));
            return actions.ToArray();
        }

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
                    paths, actions: KeepOneVersion(group.Value, versions));
            }
        }
    }
}
