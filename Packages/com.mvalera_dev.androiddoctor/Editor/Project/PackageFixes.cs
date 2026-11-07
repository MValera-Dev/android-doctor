using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace AndroidDoctor.Project
{
    /// <summary>Updates a Unity package to the version Unity recommends for this Editor, through Package Manager.</summary>
    public sealed class UpdatePackageFix : IActionFix
    {
        public string Id => FixIds.UpdatePackage;

        public string Describe(Finding f) => Messages.Get("fix.package.unknown");

        public string Describe(Finding f, FixAction action)
        {
            return string.IsNullOrEmpty(action?.Arg) ? Messages.Get("fix.package.unknown") : Messages.Get("fix.package.describe", action.Arg);
        }

        public bool Apply(Finding f, out string message) { message = Messages.Get("fix.package.unknown"); return false; }

        public bool Apply(Finding f, FixAction action, out string message)
        {
            string name = action?.Arg;
            if (string.IsNullOrEmpty(name)) { message = Messages.Get("fix.package.unknown"); return false; }

            var request = Client.Add(name);
            // The request finishes later; say how it went when it does, so a failure is never silent.
            EditorApplication.CallbackFunction watch = null;
            watch = () =>
            {
                if (!request.IsCompleted) return;
                EditorApplication.update -= watch;
                if (request.Status == StatusCode.Success)
                    Debug.Log(Messages.Get("fix.package.finished", request.Result.name, request.Result.version));
                else
                    Debug.LogWarning(Messages.Get("fix.package.failed", name, request.Error == null ? "" : request.Error.message));
            };
            EditorApplication.update += watch;
            message = Messages.Get("fix.package.started", name);
            return true;
        }
    }

    /// <summary>Opens Package Manager on a package. Changes nothing.</summary>
    public sealed class OpenPackageManagerFix : IActionFix
    {
        public string Id => FixIds.OpenPackage;

        public string Describe(Finding f) => Messages.Get("fix.openPackage.describe");
        public string Describe(Finding f, FixAction action) => Messages.Get("fix.openPackage.describe");
        public bool Apply(Finding f, out string message) => Apply(f, null, out message);

        public bool Apply(Finding f, FixAction action, out string message)
        {
            UnityEditor.PackageManager.UI.Window.Open(action?.Arg ?? "");
            message = Messages.Get("fix.openPackage.done");
            return true;
        }
    }
}
