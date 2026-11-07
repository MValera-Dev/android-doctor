using System.Collections.Generic;
using System.IO;
using System.Linq;
using AndroidDoctor.Project;

namespace AndroidDoctor.Gradle
{
    /// <summary>
    /// Which buttons a recognised Gradle error gets. Everything here is decided from the error text and from what the
    /// project actually contains; when nothing in the project matches, the finding simply has no button.
    /// </summary>
    internal static class GradleFixes
    {
        public static FixAction[] For(string patternId, object[] args, ProjectIndex project)
        {
            var actions = new List<FixAction>();
            Collect(patternId, args, project, actions);
            return actions.ToArray();
        }

        static string Arg(object[] args, int i) => i < args.Length ? (args[i] as string ?? "") : "";

        static void Collect(string id, object[] args, ProjectIndex project, List<FixAction> actions)
        {
            switch (id)
            {
                case "GRD005": // NDK version differs from the one the build asks for
                    if (project.ToolIsCustom("ndk"))
                        actions.Add(new FixAction(FixIds.UnityTools, Messages.Get("fix.unityTools.button", "NDK"), "ndk", true));
                    break;

                case "GRD006": // the same class in two libraries
                    foreach (string path in project.FindLibraries(ProjectIndex.CandidateNames(Arg(args, 1))))
                        actions.Add(new FixAction(FixIds.DisablePlugin, Messages.Get("GRD006.off", Path.GetFileName(path)), path));
                    if (project.ResolverAvailable)
                        actions.Add(new FixAction(FixIds.ResetResolver, Messages.Get("GRD006.resolver")));
                    break;

                case "GRD007": // android:exported missing
                    foreach (string manifest in project.ManifestsMissingExported())
                        actions.Add(new FixAction(FixIds.Exported, Messages.Get("PRJ006.fix"), manifest, true));
                    break;

                case "GRD008": // two plugins ship the same .so
                    foreach (string path in project.FindNativeLibraries(Path.GetFileName(Arg(args, 1))))
                        actions.Add(new FixAction(FixIds.DisablePlugin, Messages.Get("GRD006.off", path), path));
                    break;

                case "GRD009": // Gradle out of memory
                    actions.Add(new FixAction(FixIds.Heap, Messages.Get("ENV010.fix"), null, true));
                    break;

                case "GRD011": // a library needs a higher minimum API
                    int level;
                    if (int.TryParse(Arg(args, 1), out level))
                        actions.Add(new FixAction(FixIds.MinApi, Messages.Get("GRD011.fix", level), level.ToString(), true));
                    break;

                case "GRD014": // a custom template does not compile
                    var templates = project.CustomTemplates();
                    if (templates.Count > 0)
                        actions.Add(new FixAction(FixIds.ResetTemplates, Messages.Get("PRJ005.fix"),
                            string.Join(ResetTemplatesFix.Separator.ToString(), templates)));
                    break;

                case "GRD021": // a removed Gradle property is still set
                    if (project.HasRemovedGradleProperties())
                    {
                        actions.Add(new FixAction(FixIds.GradleProperties, Messages.Get("PRJ010.fix"), GradlePropertiesFix.TemplatePath, true));
                        actions.Add(new FixAction(FixIds.ResetTemplates, Messages.Get("PRJ010.reset"), GradlePropertiesFix.TemplatePath));
                    }
                    break;

                case "GRD015": // ANDROID_SDK_HOME breaks the Android plugin
                    actions.Add(new FixAction(FixIds.ClearEnvironmentVariable, Messages.Get("ENV008.fix", "ANDROID_SDK_HOME"), "ANDROID_SDK_HOME", true));
                    break;

                case GradleTranslator.UnknownId: // not recognised: the usual way out is a clean slate
                    actions.Add(new FixAction(FixIds.CleanCache, Messages.Get("GRD000.clean")));
                    break;
            }
        }
    }
}
