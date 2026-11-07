using System.IO;
using System.Text;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>Rewrites gradleTemplate.properties without the properties Gradle no longer accepts. The action argument is its path.</summary>
    public sealed class GradlePropertiesFix : IActionFix
    {
        public const string TemplatePath = "Assets/Plugins/Android/gradleTemplate.properties";
        public string Id => FixIds.GradleProperties;

        static string Target(FixAction action) => string.IsNullOrEmpty(action?.Arg) ? TemplatePath : action.Arg;
        static string FullPath(string relative) => Path.Combine(FixBackup.ProjectPath, relative);

        public string Describe(Finding f) => Describe(f, null);

        public string Describe(Finding f, FixAction action)
        {
            string path = Target(action);
            if (!File.Exists(FullPath(path))) return Messages.Get("fix.gradleProps.unknown");
            var removed = GradleProperties.FindRemoved(File.ReadAllText(FullPath(path)));
            return Messages.Get("fix.gradleProps.describe", path, string.Join("\n", removed.ConvertAll(l => "  " + l)));
        }

        public bool Apply(Finding f, out string message) => Apply(f, null, out message);

        public bool Apply(Finding f, FixAction action, out string message)
        {
            string path = Target(action);
            if (!File.Exists(FullPath(path))) { message = Messages.Get("fix.gradleProps.unknown"); return false; }

            int changed;
            string edited = GradleProperties.Migrate(File.ReadAllText(FullPath(path)), out changed);
            if (changed == 0) { message = Messages.Get("fix.gradleProps.nothing", path); return false; }

            string backup = FixBackup.Save(path);
            File.WriteAllText(FullPath(path), edited, new UTF8Encoding(false));
            AssetDatabase.Refresh();
            message = Messages.Get("fix.gradleProps.done", changed, path, backup);
            return true;
        }
    }
}
