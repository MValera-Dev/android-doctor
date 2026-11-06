using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidDoctor.Native
{
    /// <summary>
    /// NAT001: 64-bit libraries whose LOAD segments are not aligned to 16 KB, grouped by owner. Also reports,
    /// as Info, the libraries that could not be read and the 32-bit ones that are not judged.
    /// </summary>
    public sealed class NativeAlignmentRule : IRule
    {
        public string Id => "NAT001";
        public Category Category => Category.Native;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx, NativeLibScanner.Get(ctx));

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, IList<NativeLib> libs)
        {
            var misaligned = libs.Where(l => l.Is64Bit && l.Elf != null && !l.Elf.Is16KbLoadAligned);
            foreach (var group in misaligned.GroupBy(l => l.Owner).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                var items = group.OrderBy(l => l.Location, StringComparer.Ordinal).ToList();
                yield return new Finding("NAT001", Category.Native, Severity.Error,
                    Messages.Get("NAT001.title", group.Key, items.Count), Detail(ctx, group.Key),
                    items.Select(l => Messages.Get("NAT001.evidence", l.Name, l.Abi, l.Elf.MinLoadAlign().ToString("X"), l.Location)).ToList());
            }

            var unreadable = libs.Where(l => l.ReadError != null).OrderBy(l => l.Location, StringComparer.Ordinal).ToList();
            if (unreadable.Count > 0)
            {
                yield return new Finding("NAT001", Category.Native, Severity.Info,
                    Messages.Get("NAT001.unreadable.title", unreadable.Count), Messages.Get("NAT001.unreadable.detail"),
                    unreadable.Select(l => Messages.Get("NAT001.unreadable.evidence", l.Location, l.ReadError)).ToList());
            }

            int count32 = libs.Count(l => l.Is32Bit);
            if (count32 > 0)
            {
                yield return new Finding("NAT001", Category.Native, Severity.Info,
                    Messages.Get("NAT001.32bit.title", count32), Messages.Get("NAT001.32bit.detail"));
            }
        }

        // When the owner is a UPM package, say which version is installed so the user knows what to update.
        static string Detail(RuleContext ctx, string owner)
        {
            var package = ctx.Packages.FirstOrDefault(p => "Packages/" + p.Name == owner);
            return package == null
                ? Messages.Get("NAT001.detail")
                : Messages.Get("NAT001.detail.package", package.Name, package.Version);
        }
    }
}
