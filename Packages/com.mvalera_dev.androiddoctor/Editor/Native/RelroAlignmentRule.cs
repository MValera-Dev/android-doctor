using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidDoctor.Native
{
    /// <summary>NAT002: 64-bit libraries whose GNU_RELRO segment is not aligned the way Google's guide accepts.</summary>
    public sealed class RelroAlignmentRule : IRule
    {
        public string Id => "NAT002";
        public Category Category => Category.Native;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx, NativeLibScanner.Get(ctx));

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, IList<NativeLib> libs)
        {
            var bad = libs.Where(l => l.Is64Bit && l.Elf != null && !l.Elf.IsRelroAligned16Kb);
            foreach (var group in bad.GroupBy(l => l.Owner).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                var items = group.OrderBy(l => l.Location, StringComparer.Ordinal).ToList();
                yield return new Finding("NAT002", Category.Native, Severity.Warning,
                    Messages.Get("NAT002.title", group.Key, items.Count), Messages.Get("NAT002.detail"),
                    items.Select(l => Messages.Get("NAT002.evidence", l.Name, l.Abi,
                        (l.Elf.Relro.Value.VAddr + l.Elf.Relro.Value.MemSize).ToString("X"), l.Location)).ToList());
            }
        }
    }
}
