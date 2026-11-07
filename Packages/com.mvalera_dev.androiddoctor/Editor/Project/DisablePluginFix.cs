using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>
    /// Turns a plugin file (.aar, .jar, .so) off for Android, so the build stops seeing it. The file stays in the
    /// project and the change is one checkbox in the Inspector to undo. The action argument lists the files, separated by "|".
    /// </summary>
    public sealed class DisablePluginFix : IActionFix
    {
        public const char Separator = '|';
        public string Id => FixIds.DisablePlugin;

        static List<string> Paths(FixAction action)
        {
            return (action?.Arg ?? "").Split(Separator).Where(p => p.Length > 0).ToList();
        }

        public string Describe(Finding f) => Messages.Get("fix.disablePlugin.unknown");

        public string Describe(Finding f, FixAction action)
        {
            var paths = Paths(action);
            if (paths.Count == 0) return Messages.Get("fix.disablePlugin.unknown");
            return Messages.Get("fix.disablePlugin.describe", string.Join("\n", paths.Select(p => "  " + p)));
        }

        public bool Apply(Finding f, out string message) { message = Messages.Get("fix.disablePlugin.unknown"); return false; }

        public bool Apply(Finding f, FixAction action, out string message)
        {
            var done = new List<string>();
            var failed = new List<string>();
            foreach (string path in Paths(action))
            {
                var importer = AssetImporter.GetAtPath(path) as PluginImporter;
                if (importer == null) { failed.Add(path); continue; }
                // A plugin that is "any platform" is excluded per platform; otherwise it is simply not compatible.
                if (importer.GetCompatibleWithAnyPlatform()) importer.SetExcludeFromAnyPlatform(BuildTarget.Android, true);
                else importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                importer.SaveAndReimport();
                done.Add(path);
            }
            message = done.Count > 0 ? Messages.Get("fix.disablePlugin.done", done.Count, string.Join(", ", done.Select(System.IO.Path.GetFileName))) : "";
            if (failed.Count > 0) message += (message.Length > 0 ? "\n" : "") + Messages.Get("fix.disablePlugin.notFound", string.Join(", ", failed));
            return done.Count > 0;
        }
    }
}
