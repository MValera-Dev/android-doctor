using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>Opens a page of Project Settings (the action argument, "Project/Player" by default). Changes nothing.</summary>
    public sealed class OpenPlayerSettingsFix : IActionFix
    {
        public string Id => FixIds.OpenPlayerSettings;

        public string Describe(Finding f) => Messages.Get("fix.openSettings.describe");
        public string Describe(Finding f, FixAction action) => Messages.Get("fix.openSettings.describe");
        public bool Apply(Finding f, out string message) => Apply(f, null, out message);

        public bool Apply(Finding f, FixAction action, out string message)
        {
            SettingsService.OpenProjectSettings(string.IsNullOrEmpty(action?.Arg) ? "Project/Player" : action.Arg);
            message = Messages.Get("fix.openSettings.done");
            return true;
        }
    }
}
