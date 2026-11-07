using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AndroidDoctor.Project
{
    /// <summary>
    /// Adds android:exported to the components of one manifest that have an intent filter and no value. The action
    /// argument is the manifest's project-relative path. The original is copied to the backup folder first.
    /// </summary>
    public sealed class SetExportedFix : IActionFix
    {
        public string Id => FixIds.Exported;

        static string FullPath(string relative) => Path.Combine(FixBackup.ProjectPath, relative);

        public string Describe(Finding f) => Messages.Get("fix.exported.unknown");

        public string Describe(Finding f, FixAction action)
        {
            string relative = action?.Arg;
            if (string.IsNullOrEmpty(relative) || !File.Exists(FullPath(relative))) return Messages.Get("fix.exported.unknown");
            var lines = ManifestExportedChecker.FindMissing(File.ReadAllText(FullPath(relative))).Select(c =>
                "  " + c.Tag + " " + (c.Name.Length > 0 ? c.Name : "?") + ": android:exported=\"" + (c.SuggestedExported ? "true" : "false")
                + "\" (" + Messages.Get("fix.exported.reason." + c.Reason) + ")");
            return Messages.Get("fix.exported.describe", relative, string.Join("\n", lines));
        }

        public bool Apply(Finding f, out string message) { message = Messages.Get("fix.exported.unknown"); return false; }

        public bool Apply(Finding f, FixAction action, out string message)
        {
            string relative = action?.Arg;
            if (string.IsNullOrEmpty(relative) || !File.Exists(FullPath(relative))) { message = Messages.Get("fix.exported.unknown"); return false; }

            string original = File.ReadAllText(FullPath(relative));
            string edited;
            int count;
            if (!TryAdd(original, out edited, out count)) { message = Messages.Get("fix.exported.cannotEdit", relative); return false; }
            if (count == 0) { message = Messages.Get("fix.exported.nothing", relative); return false; }

            string backup = FixBackup.Save(relative);
            File.WriteAllText(FullPath(relative), edited, new UTF8Encoding(false));
            UnityEditor.AssetDatabase.Refresh();
            message = Messages.Get("fix.exported.done", count, relative, backup);
            return true;
        }

        /// <summary>
        /// Inserts the attribute right after the element name, editing the text itself so the rest of the file keeps
        /// its formatting. Works from the last component to the first so earlier line numbers stay valid.
        /// </summary>
        internal static bool TryAdd(string xml, out string edited, out int count)
        {
            edited = xml; count = 0;
            if (xml.IndexOf("xmlns:android=", StringComparison.Ordinal) < 0) return false; // we write "android:exported"

            var missing = ManifestExportedChecker.FindMissing(xml).Where(c => c.Line > 0).OrderByDescending(c => c.Line).ToList();
            var lines = xml.Replace("\r\n", "\n").Split('\n').ToList();
            bool crlf = xml.Contains("\r\n");
            foreach (var component in missing)
            {
                int index = component.Line - 1;
                if (index < 0 || index >= lines.Count) return false;
                string line = lines[index];
                string open = "<" + component.Tag;
                int at = line.IndexOf(open, StringComparison.Ordinal);
                if (at < 0) return false;
                int insertAt = at + open.Length;
                string value = component.SuggestedExported ? "true" : "false";
                lines[index] = line.Substring(0, insertAt) + " android:exported=\"" + value + "\"" + line.Substring(insertAt);
                count++;
            }
            edited = string.Join(crlf ? "\r\n" : "\n", lines);
            return true;
        }
    }
}
