using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidDoctor.Environment
{
    /// <summary>
    /// Puts the JDK, SDK, NDK or Gradle back to the ones installed with Unity, which is what Unity tests against.
    /// The action argument names one tool ("jdk", "sdk", "ndk", "gradle"); without it, every custom one.
    /// </summary>
    public sealed class UseUnityToolsFix : IActionFix
    {
        public string Id => FixIds.UnityTools;

        internal static string DisplayName(string kind)
        {
            switch (kind)
            {
                case "jdk": return "JDK";
                case "sdk": return "Android SDK";
                case "ndk": return "NDK";
                default: return "Gradle";
            }
        }

        /// <summary>
        /// The buttons for a problem with one tool: switch to Unity's own (only when this one is custom, since only
        /// then does the switch change anything), and open Preferences for people who want to look first.
        /// </summary>
        internal static FixAction[] ActionsFor(string kind, bool isCustom)
        {
            var open = new FixAction(FixIds.OpenExternalTools, Messages.Get("fix.externalTools.button"));
            if (!isCustom) return new[] { open };
            return new[]
            {
                new FixAction(FixIds.UnityTools, Messages.Get("fix.unityTools.button", DisplayName(kind)), kind, true),
                open,
            };
        }

        /// <summary>The tools to change: the one named, or every custom one.</summary>
        static List<string> Targets(FixAction action)
        {
            var kinds = string.IsNullOrEmpty(action?.Arg) ? ExternalToolsSettings.Kinds : new[] { action.Arg };
            return kinds.Where(k => !ExternalToolsSettings.IsEmbedded(k)).ToList();
        }

        public string Describe(Finding f) => Describe(f, null);

        public string Describe(Finding f, FixAction action)
        {
            var targets = Targets(action);
            if (targets.Count == 0) return Messages.Get("fix.unityTools.nothing");
            return string.Join("\n", targets.Select(k => Messages.Get("fix.unityTools.line",
                DisplayName(k), ExternalToolsSettings.CurrentPath(k), ExternalToolsSettings.EmbeddedPath(k))));
        }

        public bool Apply(Finding f, out string message) => Apply(f, null, out message);

        public bool Apply(Finding f, FixAction action, out string message)
        {
            var targets = Targets(action);
            if (targets.Count == 0) { message = Messages.Get("fix.unityTools.nothing"); return false; }

            var done = new List<string>();
            var failed = new List<string>();
            foreach (string kind in targets)
            {
                try { ExternalToolsSettings.UseUnityTool(kind); done.Add(DisplayName(kind)); }
                catch (Exception e) { failed.Add(DisplayName(kind) + ": " + e.Message); }
            }
            message = done.Count > 0 ? Messages.Get("fix.unityTools.done", string.Join(", ", done)) : "";
            if (failed.Count > 0) message += (message.Length > 0 ? "\n" : "") + string.Join("\n", failed);
            return done.Count > 0;
        }
    }
}
