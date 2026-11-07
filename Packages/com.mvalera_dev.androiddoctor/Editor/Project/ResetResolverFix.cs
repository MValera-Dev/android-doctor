using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>
    /// Runs the External Dependency Manager's own "Delete Resolved Libraries" and "Force Resolve", the answer forum
    /// threads give for "Duplicate class". It drives the package's menu, so it works with whatever version is installed.
    /// </summary>
    public sealed class ResetResolverFix : IFix
    {
        const string Menu = "Assets/External Dependency Manager/Android Resolver/";
        public string Id => FixIds.ResetResolver;

        public string Describe(Finding f) => Messages.Get("fix.resolver.describe");

        public bool Apply(Finding f, out string message)
        {
            bool deleted = EditorApplication.ExecuteMenuItem(Menu + "Delete Resolved Libraries");
            bool resolved = EditorApplication.ExecuteMenuItem(Menu + "Force Resolve");
            if (!deleted && !resolved) { message = Messages.Get("fix.resolver.missing"); return false; }
            message = Messages.Get("fix.resolver.done");
            return true;
        }
    }
}
