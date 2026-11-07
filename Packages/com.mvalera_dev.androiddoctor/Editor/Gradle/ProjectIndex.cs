using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AndroidDoctor.Environment;
using AndroidDoctor.Project;
using UnityEditor;

namespace AndroidDoctor.Gradle
{
    /// <summary>
    /// What the translator needs to know about the project to turn a Gradle message into a button: which files in
    /// Assets match a library named in an error, which manifests lack android:exported, which tools are custom.
    /// Virtual so tests can describe a project without touching the real one.
    /// </summary>
    public class ProjectIndex
    {
        static string AssetsPath => UnityEngine.Application.dataPath;
        static string ProjectPath => Path.GetDirectoryName(UnityEngine.Application.dataPath);

        /// <summary>Library files (.aar/.jar) in Assets whose name, without "jetified-", is one of the candidates.</summary>
        public virtual List<string> FindLibraries(IList<string> candidates)
        {
            var wanted = new HashSet<string>(candidates, StringComparer.OrdinalIgnoreCase);
            return Find(file =>
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".aar" && ext != ".jar") return false;
                string stem = Path.GetFileNameWithoutExtension(file);
                if (stem.StartsWith("jetified-", StringComparison.OrdinalIgnoreCase)) stem = stem.Substring("jetified-".Length);
                return wanted.Contains(stem) || wanted.Contains(Path.GetFileName(file));
            });
        }

        /// <summary>Native libraries in Assets with exactly this file name, for example "libfoo.so".</summary>
        public virtual List<string> FindNativeLibraries(string fileName)
        {
            return Find(file => string.Equals(Path.GetFileName(file), fileName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Manifests in Assets that have a component with an intent filter and no android:exported.</summary>
        public virtual List<string> ManifestsMissingExported()
        {
            var result = new List<string>();
            foreach (string manifest in ManifestExportedRule.FindManifests(AssetsPath))
            {
                string text;
                try { text = File.ReadAllText(manifest); } catch (IOException) { continue; } catch (UnauthorizedAccessException) { continue; }
                if (ManifestExportedChecker.FindMissing(text).Count > 0)
                    result.Add(ProjectFileWalker.ToProjectRelative(ProjectPath, manifest));
            }
            return result;
        }

        /// <summary>True when the tool is not the one installed with Unity, so switching changes something.</summary>
        public virtual bool ToolIsCustom(string kind)
        {
            return ExternalToolsSettings.Available && !ExternalToolsSettings.IsEmbedded(kind);
        }

        static readonly string[] TemplateNames =
        {
            "mainTemplate.gradle", "launcherTemplate.gradle", "baseProjectTemplate.gradle",
            "settingsTemplate.gradle", "gradleTemplate.properties",
        };

        /// <summary>The custom Gradle templates the project has, as project-relative paths. A Gradle error names a generated file, not which template caused it.</summary>
        public virtual List<string> CustomTemplates()
        {
            return TemplateNames.Select(n => "Assets/Plugins/Android/" + n)
                .Where(p => File.Exists(Path.Combine(ProjectPath, p))).ToList();
        }

        /// <summary>True when gradleTemplate.properties still sets a property the Android Gradle plugin removed.</summary>
        public virtual bool HasRemovedGradleProperties()
        {
            string file = Path.Combine(ProjectPath, GradlePropertiesFix.TemplatePath);
            try { return File.Exists(file) && GradleProperties.FindRemoved(File.ReadAllText(file)).Count > 0; }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        /// <summary>True when the External Dependency Manager (EDM4U) is in the project.</summary>
        public virtual bool ResolverAvailable
        {
            get
            {
                if (Directory.Exists(Path.Combine(AssetsPath, "ExternalDependencyManager"))) return true;
                return UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages()
                    .Any(p => p.name == "com.google.external-dependency-manager");
            }
        }

        static List<string> Find(Func<string, bool> predicate)
        {
            var files = ProjectFileWalker.FindFiles(AssetsPath, predicate);
            return files.Select(f => ProjectFileWalker.ToProjectRelative(ProjectPath, f)).OrderBy(f => f, StringComparer.Ordinal).ToList();
        }

        /// <summary>
        /// The file names a "found in modules" message can refer to. Gradle names a module by coordinates,
        /// "(androidx.core:core:1.5.0)", or by file, "(foo-1.0.jar)"; EDM4U saves the first as "androidx.core.core-1.5.0.aar".
        /// </summary>
        public static List<string> CandidateNames(string modulesText)
        {
            var names = new List<string>();
            foreach (Match m in Regex.Matches(modulesText ?? "", @"\(([^)\s]+)\)"))
            {
                string inner = m.Groups[1].Value;
                string[] parts = inner.Split(':');
                if (parts.Length == 3)
                {
                    names.Add(parts[1] + "-" + parts[2]);
                    names.Add(parts[0] + "." + parts[1] + "-" + parts[2]);
                }
                else names.Add(inner);
            }
            return names;
        }
    }
}
