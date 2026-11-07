using System;
using System.IO;
using System.Reflection;
using UnityEditor;

namespace AndroidDoctor.Environment
{
    /// <summary>
    /// Reads and writes Preferences > External Tools through Unity's public AndroidExternalToolsSettings.
    /// It lives in the Android extensions assembly, which this package does not reference, so it is reached
    /// by name. Assigning null to a path puts the tool back to the one installed with Unity.
    /// </summary>
    internal static class ExternalToolsSettings
    {
        const string TypeName = "UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions";
        const BindingFlags StaticPublic = BindingFlags.Static | BindingFlags.Public;

        public static readonly string[] Kinds = { "jdk", "sdk", "ndk", "gradle" };

        static Type SettingsType() => Type.GetType(TypeName, false);

        public static bool Available => SettingsType() != null;

        static string PathProperty(string kind)
        {
            switch (kind)
            {
                case "jdk": return "jdkRootPath";
                case "sdk": return "sdkRootPath";
                case "ndk": return "ndkRootPath";
                case "gradle": return "gradlePath";
                default: throw new ArgumentException("Unknown tool: " + kind);
            }
        }

        /// <summary>The folder Unity uses for this tool right now, or null when it cannot be read.</summary>
        public static string CurrentPath(string kind)
        {
            var p = SettingsType()?.GetProperty(PathProperty(kind), StaticPublic);
            try { return p == null ? null : p.GetValue(null) as string; }
            catch (Exception) { return null; }
        }

        /// <summary>The folder of the tool installed with this Unity, whether or not it exists.</summary>
        public static string EmbeddedPath(string kind)
        {
            string sub;
            switch (kind)
            {
                case "jdk": sub = "OpenJDK"; break;
                case "sdk": sub = "SDK"; break;
                case "ndk": sub = "NDK"; break;
                default: sub = Path.Combine("Tools", "gradle"); break;
            }
            return Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer", sub);
        }

        public static bool IsEmbedded(string kind)
        {
            string current = CurrentPath(kind);
            return current != null && AndroidToolsReader.SamePath(current, EmbeddedPath(kind));
        }

        /// <summary>Puts the tool back to the one installed with Unity. Throws with Unity's own message if it cannot.</summary>
        public static void UseUnityTool(string kind)
        {
            var p = SettingsType()?.GetProperty(PathProperty(kind), StaticPublic);
            if (p == null) throw new InvalidOperationException(Messages.Get("fix.unityTools.unavailable"));
            try { p.SetValue(null, null); }
            catch (TargetInvocationException e) { throw new InvalidOperationException(e.InnerException?.Message ?? e.Message); }
        }

        public static int HeapMegabytes
        {
            get
            {
                var p = SettingsType()?.GetProperty("maxJvmHeapSize", StaticPublic);
                try { return p == null ? -1 : Convert.ToInt32(p.GetValue(null)); }
                catch (Exception) { return -1; }
            }
            set
            {
                var p = SettingsType()?.GetProperty("maxJvmHeapSize", StaticPublic);
                if (p == null) throw new InvalidOperationException(Messages.Get("fix.unityTools.unavailable"));
                p.SetValue(null, value);
            }
        }
    }
}
