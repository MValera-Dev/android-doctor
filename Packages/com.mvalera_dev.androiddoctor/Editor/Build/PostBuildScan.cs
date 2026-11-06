using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AndroidDoctor.Build
{
    /// <summary>
    /// After an Android build, opens the APK/AAB that was produced and checks what will really be uploaded.
    /// It only logs; it never fails or changes the build.
    /// </summary>
    public sealed class PostBuildScan : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1000;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android) return;
            string output = report.summary.outputPath;
            // A Gradle project export is a folder, not a file: nothing to open.
            if (string.IsNullOrEmpty(output) || !File.Exists(output)) return;

            try
            {
                BuildArtifactState.Set(output);
                var libs = new System.Collections.Generic.List<Native.NativeLib>();
                string error;
                if (!ArtifactScanner.TryScan(output, out libs, out error))
                {
                    Debug.LogWarning(Messages.Get("build.postbuild.failed", error));
                    return;
                }

                int misaligned = libs.Count(l => l.Is64Bit && l.Elf != null && !l.Elf.Is16KbLoadAligned);
                if (misaligned > 0) Debug.LogWarning(Messages.Get("build.postbuild.warning", misaligned));
                else Debug.Log(Messages.Get("build.postbuild.ok"));
            }
            catch (Exception e)
            {
                // A check after the build must never be the reason a build reports a problem.
                Debug.LogWarning(Messages.Get("build.postbuild.failed", e.Message));
            }
        }
    }
}
