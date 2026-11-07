using System;
using System.Collections.Generic;
using System.IO;

namespace AndroidDoctor.Project
{
    /// <summary>
    /// Deletes the Gradle project Unity generated for Android and its output. Unity builds both again on the next
    /// build, so this is the safe "start from a clean slate" that forum answers recommend. Nothing of the user's is touched.
    /// </summary>
    public sealed class CleanBuildCacheFix : IFix
    {
        public string Id => FixIds.CleanCache;

        static IEnumerable<string> Folders()
        {
            string project = FixBackup.ProjectPath;
            yield return Path.Combine(project, "Library", "Bee", "Android");
            yield return Path.Combine(project, "Temp", "gradleOut");
        }

        public string Describe(Finding f) => Messages.Get("fix.cleanCache.describe");

        public bool Apply(Finding f, out string message)
        {
            var removed = new List<string>();
            var failed = new List<string>();
            foreach (string folder in Folders())
            {
                if (!Directory.Exists(folder)) continue;
                try { Directory.Delete(folder, true); removed.Add(folder); }
                catch (Exception e) { failed.Add(folder + ": " + e.Message); }
            }
            if (removed.Count == 0 && failed.Count == 0) { message = Messages.Get("fix.cleanCache.nothing"); return false; }
            message = removed.Count > 0 ? Messages.Get("fix.cleanCache.done", removed.Count) : "";
            if (failed.Count > 0) message += (message.Length > 0 ? "\n" : "") + Messages.Get("fix.cleanCache.failed", string.Join("\n", failed));
            return removed.Count > 0;
        }
    }
}
