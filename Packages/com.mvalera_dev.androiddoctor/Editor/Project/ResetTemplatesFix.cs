using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>
    /// Removes custom Gradle templates so Unity uses its own, which match the Gradle and Android Gradle plugin it ships.
    /// Each file is copied to the backup folder first. The action argument lists project-relative paths, separated by "|".
    /// </summary>
    public sealed class ResetTemplatesFix : IActionFix
    {
        public const char Separator = '|';
        public string Id => FixIds.ResetTemplates;

        static List<string> Paths(FixAction action)
        {
            return (action?.Arg ?? "").Split(Separator).Where(p => p.Length > 0).ToList();
        }

        static bool Exists(string relative) => File.Exists(Path.Combine(FixBackup.ProjectPath, relative));

        public string Describe(Finding f) => Messages.Get("fix.templates.unknown");

        public string Describe(Finding f, FixAction action)
        {
            var paths = Paths(action).Where(Exists).ToList();
            if (paths.Count == 0) return Messages.Get("fix.templates.unknown");
            return Messages.Get("fix.templates.describe", string.Join("\n", paths.Select(p => "  " + p)));
        }

        public bool Apply(Finding f, out string message) { message = Messages.Get("fix.templates.unknown"); return false; }

        public bool Apply(Finding f, FixAction action, out string message)
        {
            var removed = new List<string>();
            foreach (string path in Paths(action).Where(Exists))
            {
                FixBackup.Save(path);
                if (AssetDatabase.DeleteAsset(path)) removed.Add(Path.GetFileName(path));
            }
            if (removed.Count == 0) { message = Messages.Get("fix.templates.unknown"); return false; }
            message = Messages.Get("fix.templates.done", string.Join(", ", removed), FixBackup.RunFolder);
            return true;
        }
    }
}
