using System;
using System.Collections.Generic;
using UnityEditor;

namespace AndroidDoctor
{
    /// <summary>Finds every concrete IFix with a public parameterless constructor and looks them up by id.</summary>
    public static class FixRegistry
    {
        public static List<IFix> DiscoverFixes()
        {
            var fixes = new List<IFix>();
            foreach (var t in TypeCache.GetTypesDerivedFrom<IFix>())
            {
                if (t.IsAbstract || t.IsInterface || t.GetConstructor(Type.EmptyTypes) == null) continue;
                fixes.Add((IFix)Activator.CreateInstance(t));
            }
            return fixes;
        }

        /// <summary>The fix with this id, or null if there is none (the caller must handle null).</summary>
        public static IFix Get(string fixId)
        {
            if (string.IsNullOrEmpty(fixId)) return null;
            foreach (var fix in DiscoverFixes())
                if (fix.Id == fixId) return fix;
            return null;
        }
    }
}
