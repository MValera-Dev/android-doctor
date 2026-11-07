using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>Turns on Build App Bundle, the format Google Play requires for new apps.</summary>
    public sealed class EnableAppBundleFix : IFix
    {
        public string Id => FixIds.AppBundle;

        public string Describe(Finding f) => Messages.Get("fix.appBundle.describe");

        public bool Apply(Finding f, out string message)
        {
            EditorUserBuildSettings.buildAppBundle = true;
            message = Messages.Get("fix.appBundle.done");
            return true;
        }
    }
}
