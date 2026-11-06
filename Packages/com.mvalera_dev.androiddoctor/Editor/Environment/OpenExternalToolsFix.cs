using UnityEditor;

namespace AndroidDoctor.Environment
{
    /// <summary>Opens Preferences > External Tools, where the user picks the tools installed with Unity. Changes nothing.</summary>
    public sealed class OpenExternalToolsFix : IFix
    {
        public string Id => FixIds.OpenExternalTools;

        public string Describe(Finding f) => Messages.Get("fix.externalTools.describe");

        public bool Apply(Finding f, out string message)
        {
            SettingsService.OpenUserPreferences("Preferences/External Tools");
            message = Messages.Get("fix.externalTools.done");
            return true;
        }
    }
}
