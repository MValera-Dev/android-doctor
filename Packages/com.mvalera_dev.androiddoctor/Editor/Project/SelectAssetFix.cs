using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>Selects the asset named by the finding's first evidence line in the Project window. Changes nothing.</summary>
    public sealed class SelectAssetFix : IFix
    {
        public string Id => FixIds.SelectAsset;

        static string AssetPath(Finding f) => f.Evidence.Count > 0 ? f.Evidence[0] : null;

        public string Describe(Finding f) => Messages.Get("fix.selectAsset.describe", AssetPath(f));

        public bool Apply(Finding f, out string message)
        {
            string path = AssetPath(f);
            var asset = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null) { message = Messages.Get("fix.selectAsset.notFound", path); return false; }
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            message = Messages.Get("fix.selectAsset.done", path);
            return true;
        }
    }
}
