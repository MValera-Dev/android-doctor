using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidDoctor.Native
{
    /// <summary>NAT003: a library that ships 32-bit ABIs but no 64-bit one, so a 64-bit build cannot include it.</summary>
    public sealed class Missing64BitRule : IRule
    {
        public string Id => "NAT003";
        public Category Category => Category.Native;

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx, NativeLibScanner.Get(ctx));

        internal static IEnumerable<Finding> Evaluate(RuleContext ctx, IList<NativeLib> libs)
        {
            // Same owner and same file name across ABI folders is the same library in different ABIs.
            var problems = new List<KeyValuePair<string, string>>(); // owner, "libfoo.so: armeabi-v7a, x86"
            var byLibrary = libs.Where(l => l.Abi != null).GroupBy(l => new { l.Owner, l.Name });
            foreach (var library in byLibrary)
            {
                var abis = library.Select(l => l.Abi).Distinct().OrderBy(a => a, StringComparer.Ordinal).ToList();
                if (library.Any(l => l.Is64Bit) || !library.Any(l => l.Is32Bit)) continue;
                problems.Add(new KeyValuePair<string, string>(library.Key.Owner,
                    Messages.Get("NAT003.evidence", library.Key.Name, string.Join(", ", abis))));
            }

            foreach (var group in problems.GroupBy(p => p.Key).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                var evidence = group.Select(p => p.Value).OrderBy(e => e, StringComparer.Ordinal).ToList();
                yield return new Finding("NAT003", Category.Native, Severity.Error,
                    Messages.Get("NAT003.title", group.Key, evidence.Count), Messages.Get("NAT003.detail"), evidence);
            }
        }
    }
}
