using System;
using UnityEngine;

namespace AndroidDoctor.Environment
{
    /// <summary>
    /// Raises "Maximum JVM heap size" in Preferences > External Tools. Unity passes it to Gradle on the command line,
    /// so it wins over any org.gradle.jvmargs in a properties template.
    /// </summary>
    public sealed class RaiseHeapFix : IFix
    {
        public const int MinimumHealthy = 2048;
        const int Floor = 4096;
        const int Ceiling = 8192;

        public string Id => FixIds.Heap;

        /// <summary>Doubles the heap, at least 4 GB, at most 8 GB and never more than 60% of the machine's memory.</summary>
        internal static int Target(int currentMegabytes, int systemMegabytes)
        {
            int wanted = Math.Max(currentMegabytes * 2, Floor);
            int cap = systemMegabytes > 0 ? Math.Max((int)(systemMegabytes * 0.6), MinimumHealthy) : Ceiling;
            return Math.Min(Math.Min(wanted, Ceiling), cap);
        }

        public string Describe(Finding f)
        {
            int current = ExternalToolsSettings.HeapMegabytes;
            return Messages.Get("fix.heap.describe", current, Target(current, SystemInfo.systemMemorySize));
        }

        public bool Apply(Finding f, out string message)
        {
            int current = ExternalToolsSettings.HeapMegabytes;
            if (current < 0) { message = Messages.Get("fix.unityTools.unavailable"); return false; }
            int target = Target(current, SystemInfo.systemMemorySize);
            if (target <= current) { message = Messages.Get("fix.heap.already", current); return false; }
            ExternalToolsSettings.HeapMegabytes = target;
            message = Messages.Get("fix.heap.done", current, target);
            return true;
        }
    }
}
