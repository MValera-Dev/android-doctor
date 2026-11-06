using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AndroidDoctor
{
    /// <summary>Compares package versions by their numeric part only: "1.9.0-pre.1" is treated as "1.9.0".</summary>
    public static class VersionCompare
    {
        static readonly Regex Numeric = new Regex(@"^\s*(\d+(?:\.\d+)*)");

        /// <summary>
        /// True when a is a lower version than b. False when they are equal or when either cannot be parsed:
        /// not knowing is never reported as "older".
        /// </summary>
        public static bool IsLess(string a, string b)
        {
            List<int> left = Parse(a), right = Parse(b);
            if (left == null || right == null) return false;

            int count = System.Math.Max(left.Count, right.Count);
            for (int i = 0; i < count; i++)
            {
                int l = i < left.Count ? left[i] : 0;
                int r = i < right.Count ? right[i] : 0;
                if (l != r) return l < r;
            }
            return false;
        }

        static List<int> Parse(string version)
        {
            if (string.IsNullOrEmpty(version)) return null;
            var m = Numeric.Match(version);
            if (!m.Success) return null;
            var parts = new List<int>();
            foreach (string piece in m.Groups[1].Value.Split('.'))
            {
                int n;
                if (!int.TryParse(piece, out n)) return null; // overflow: treat as unparseable
                parts.Add(n);
            }
            return parts;
        }
    }
}
