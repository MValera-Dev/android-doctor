using System;
using System.Text.RegularExpressions;

namespace AndroidDoctor
{
    public readonly struct UnityVersion : IComparable<UnityVersion>
    {
        public readonly int Major, Minor, Patch;
        public UnityVersion(int major, int minor, int patch) { Major = major; Minor = minor; Patch = patch; }

        // Application.unityVersion: "6000.3.2f1", "2022.3.61f1"
        public static UnityVersion Parse(string s)
        {
            var m = Regex.Match(s ?? "", @"^(\d+)\.(\d+)\.(\d+)");
            if (!m.Success) return new UnityVersion(0, 0, 0);
            return new UnityVersion(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value));
        }

        public bool AtLeast(int major, int minor) => Major > major || (Major == major && Minor >= minor);
        public string Key => Major + "." + Minor; // "6000.3"
        public int CompareTo(UnityVersion o)
        {
            int c = Major.CompareTo(o.Major); if (c != 0) return c;
            c = Minor.CompareTo(o.Minor); return c != 0 ? c : Patch.CompareTo(o.Patch);
        }
        public override string ToString() => Major + "." + Minor + "." + Patch;
    }
}
