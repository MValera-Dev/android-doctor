using System;
using System.Globalization;
using UnityEngine;

namespace AndroidDoctor
{
    /// <summary>
    /// One row of compat.json. Unknown values: null for strings, -1 for integers.
    /// JsonUtility may read a JSON null string as "", so treat null and empty alike (string.IsNullOrEmpty).
    /// </summary>
    [Serializable]
    public sealed class CompatRow
    {
        public string match;      // "6000.5": prefix of Major.Minor
        public int jdk = -1;
        public string ndk;
        public string buildTools;
        public string gradle;
        public string agp;
        public int minSdk = -1;
    }

    [Serializable]
    public sealed class CompatTable
    {
        public string asOf;
        public CompatRow[] unity = new CompatRow[0];

        /// <summary>Loads Editor/Data/compat.json from the package. Null if it cannot be read.</summary>
        public static CompatTable Load() => Parse(DataFiles.ReadText("compat.json"));

        /// <summary>Parses compat.json text. Null if the text is empty or not valid JSON.</summary>
        public static CompatTable Parse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var table = JsonUtility.FromJson<CompatTable>(json);
                if (table != null && table.unity == null) table.unity = new CompatRow[0];
                return table;
            }
            catch (ArgumentException) { return null; }
        }

        /// <summary>
        /// The most specific row for this Unity version: same major, highest Major.Minor not above the
        /// version. A newer minor than any row inherits the closest older row (6000.6 uses 6000.5).
        /// Null when no row applies.
        /// </summary>
        public CompatRow Find(UnityVersion v)
        {
            CompatRow best = null;
            int bestMinor = -1;
            foreach (var row in unity)
            {
                int major, minor;
                if (row == null || !TryParseMatch(row.match, out major, out minor)) continue;
                if (major != v.Major || minor > v.Minor) continue;
                if (best == null || minor > bestMinor) { best = row; bestMinor = minor; }
            }
            return best;
        }

        static bool TryParseMatch(string match, out int major, out int minor)
        {
            major = 0; minor = 0;
            if (string.IsNullOrEmpty(match)) return false;
            var parts = match.Split('.');
            if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out major)) return false;
            if (parts.Length > 1 && !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out minor)) return false;
            return true;
        }
    }
}
