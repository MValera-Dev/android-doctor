using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndroidDoctor.UI
{
    /// <summary>Loads the stylesheet and the Editor's own icons, so the window looks like the rest of Unity.</summary>
    internal static class UiStyles
    {
        const string StyleSheetPath = "Packages/com.mvalera_dev.androiddoctor/Editor/UI/AndroidDoctor.uss";

        /// <summary>Adds the stylesheet to a root element. Everything inside it is styled. Quietly does nothing if it is not imported yet.</summary>
        public static void Apply(VisualElement root)
        {
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (sheet != null && !root.styleSheets.Contains(sheet)) root.styleSheets.Add(sheet);
        }

        /// <summary>The class a row gets for its severity.</summary>
        public static string ClassFor(Severity severity)
        {
            switch (severity)
            {
                case Severity.Error: return "ad-error";
                case Severity.Warning: return "ad-warning";
                default: return "ad-info";
            }
        }

        /// <summary>The Console's own icon for a severity (16 px). Null if this Unity version has no such icon.</summary>
        public static Texture2D Icon(Severity severity, bool large = false)
        {
            string name;
            switch (severity)
            {
                case Severity.Error: name = "console.erroricon"; break;
                case Severity.Warning: name = "console.warnicon"; break;
                default: name = "console.infoicon"; break;
            }
            return Load(large ? name : name + ".sml");
        }

        /// <summary>The check mark Unity's Test Runner uses for "passed".</summary>
        public static Texture2D PassedIcon() => Load("TestPassed");

        static Texture2D Load(string name)
        {
            var content = EditorGUIUtility.IconContent(name);
            return content == null ? null : content.image as Texture2D;
        }
    }
}
