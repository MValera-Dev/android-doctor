using System;
using System.Collections.Generic;
using System.Linq;
using AndroidDoctor.Native;

namespace AndroidDoctor.Build
{
    /// <summary>
    /// BLD001: 64-bit libraries inside the built APK/AAB whose LOAD segments are not aligned to 16 KB. Each one
    /// is linked back to the project scan by file name and ABI, so the finding can name who ships it.
    /// </summary>
    public sealed class BuildAlignmentRule : IRule
    {
        const string BuildLibsKey = "buildLibs";

        public string Id => "BLD001";
        public Category Category => Category.Build;

        public IEnumerable<Finding> Run(RuleContext ctx)
        {
            if (string.IsNullOrEmpty(ctx.ArtifactPath)) return new Finding[0];

            var scan = ctx.GetOrCompute(BuildLibsKey, () =>
            {
                List<NativeLib> libs; string error;
                bool ok = ArtifactScanner.TryScan(ctx.ArtifactPath, out libs, out error);
                return new ScanResult { Libs = libs, Error = ok ? null : error };
            });

            if (scan.Error != null)
            {
                return new[]
                {
                    new Finding("BLD001", Category.Build, Severity.Info,
                        Messages.Get("BLD001.unreadable.title"), Messages.Get("BLD001.unreadable.detail"),
                        new[] { Messages.Get("BLD001.unreadable.evidence", ctx.ArtifactPath, scan.Error) }),
                };
            }
            return Evaluate(ctx, scan.Libs, NativeLibScanner.Get(ctx)).ToList();
        }

        sealed class ScanResult { public List<NativeLib> Libs; public string Error; }

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, IList<NativeLib> buildLibs, IList<NativeLib> projectLibs)
        {
            var bad = buildLibs.Where(l => l.Is64Bit && l.Elf != null && !l.Elf.Is16KbLoadAligned);

            // owner label -> libraries; the owner comes from the project scan, matched by name and ABI.
            var groups = new Dictionary<string, List<NativeLib>>(StringComparer.Ordinal);
            foreach (var lib in bad)
            {
                var owners = projectLibs.Where(p => p.Name == lib.Name && p.Abi == lib.Abi)
                    .Select(p => p.Owner).Distinct().OrderBy(o => o, StringComparer.Ordinal).ToList();
                string key = owners.Count == 0 ? null : string.Join(", ", owners);

                List<NativeLib> list;
                string groupKey = key ?? "";
                if (!groups.TryGetValue(groupKey, out list)) groups[groupKey] = list = new List<NativeLib>();
                list.Add(lib);
            }

            // Known owners first (alphabetical), then the libraries that are not in the project.
            foreach (var group in groups.OrderBy(g => g.Key.Length == 0 ? 1 : 0).ThenBy(g => g.Key, StringComparer.Ordinal))
            {
                bool known = group.Key.Length > 0;
                string owner = known ? group.Key : Messages.Get("BLD001.owner.unknown");
                var items = group.Value.OrderBy(l => l.Location, StringComparer.Ordinal).ToList();
                yield return new Finding("BLD001", Category.Build, Severity.Error,
                    Messages.Get("BLD001.title", owner, items.Count),
                    Messages.Get(known ? "BLD001.detail.known" : "BLD001.detail.unknown"),
                    items.Select(l => Messages.Get("BLD001.evidence", l.Name, l.Abi,
                        l.Elf.MinLoadAlign().ToString("X"), l.Location)).ToList());
            }
        }
    }
}
