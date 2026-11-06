using System;
using System.IO;
using System.Reflection;
using UnityEditor;

namespace AndroidDoctor.Environment
{
    public enum ToolSource { Unknown, Embedded, Custom }

    public sealed class ToolPath
    {
        public string Path;
        public ToolSource Source;
        public bool Exists => !string.IsNullOrEmpty(Path) && (Directory.Exists(Path) || File.Exists(Path));
    }

    public sealed class AndroidTools
    {
        public ToolPath Jdk, Sdk, Ndk, Gradle;
    }

    /// <summary>
    /// Reads the JDK, SDK, NDK and Gradle paths by reflection: UnityEditor.Android.AndroidExternalToolsSettings
    /// only exists when the Android module is installed, so a direct reference would break compilation in
    /// editors without it. If the API is missing or fails, the path stays Unknown and dependent rules skip.
    /// </summary>
    public static class AndroidToolsReader
    {
        const string SettingsType = "UnityEditor.Android.AndroidExternalToolsSettings";
        const string SettingsAssembly = "UnityEditor.Android.Extensions";
        const BindingFlags StaticPublic = BindingFlags.Public | BindingFlags.Static;

        public static AndroidTools Read()
        {
            string engineDir = EngineDir(); // .../PlaybackEngines/AndroidPlayer (or null)
            return new AndroidTools
            {
                Jdk = Resolve(ReadSetting("jdkRootPath"), engineDir, "OpenJDK"),
                Sdk = Resolve(ReadSetting("sdkRootPath"), engineDir, "SDK"),
                Ndk = Resolve(ReadSetting("ndkRootPath"), engineDir, "NDK"),
                Gradle = Resolve(ReadGradlePath(), engineDir, System.IO.Path.Combine("Tools", "gradle")),
            };
        }

        static string EngineDir()
        {
            try
            {
                string d = BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, BuildOptions.None);
                return string.IsNullOrEmpty(d) ? null : d;
            }
            catch (Exception) { return null; }
        }

        static ToolPath Resolve(string configured, string engineDir, string embeddedSub)
        {
            string embedded = engineDir == null ? null : System.IO.Path.Combine(engineDir, embeddedSub);
            if (!string.IsNullOrEmpty(configured))
            {
                bool same = embedded != null && SamePath(configured, embedded);
                return new ToolPath { Path = configured, Source = same ? ToolSource.Embedded : ToolSource.Custom };
            }
            if (embedded != null && Directory.Exists(embedded))
                return new ToolPath { Path = embedded, Source = ToolSource.Embedded };
            return new ToolPath { Path = null, Source = ToolSource.Unknown };
        }

        internal static bool SamePath(string a, string b)
        {
            try
            {
                return string.Equals(
                    System.IO.Path.GetFullPath(a).TrimEnd('/', '\\'),
                    System.IO.Path.GetFullPath(b).TrimEnd('/', '\\'),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception) { return false; }
        }

        static Type FindSettingsType()
        {
            // Fast path: the type lives in the Android extensions assembly, resolvable by qualified name.
            var type = Type.GetType(SettingsType + ", " + SettingsAssembly, false);
            if (type != null) return type;

            // Fallback in case a Unity version moves the type to another assembly. This runs once per
            // diagnosis, so the analyzer's concern about unloaded assemblies does not apply.
#pragma warning disable UAC0005
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
#pragma warning restore UAC0005
            {
                type = asm.GetType(SettingsType, false);
                if (type != null) return type;
            }
            return null;
        }

        static string ReadSetting(string property)
        {
            var type = FindSettingsType();
            return type == null ? null : ReadStaticString(type, property);
        }

        // Unity 6000.6 still exposes the flat gradlePath, but the nested Gradle type is the newer shape.
        // Try the flat property first, then a static "path" on the nested type.
        static string ReadGradlePath()
        {
            var type = FindSettingsType();
            if (type == null) return null;
            string flat = ReadStaticString(type, "gradlePath");
            if (!string.IsNullOrEmpty(flat)) return flat;
            var nested = type.GetNestedType("Gradle", BindingFlags.Public | BindingFlags.NonPublic);
            return nested == null ? null : ReadStaticString(nested, "path");
        }

        static string ReadStaticString(Type type, string property)
        {
            var p = type.GetProperty(property, StaticPublic);
            if (p == null) return null;
            try { return p.GetValue(null) as string; }
            catch (Exception) { return null; }
        }
    }
}
