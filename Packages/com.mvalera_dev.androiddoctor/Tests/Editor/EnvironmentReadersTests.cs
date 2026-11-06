using NUnit.Framework;
using AndroidDoctor.Environment;

namespace AndroidDoctor.Tests
{
    public class EnvironmentReadersTests
    {
        // --- JdkRelease ---

        [Test] public void JdkRelease_ReadsJavaVersionFromReleaseFile()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("release", "IMPLEMENTOR=\"Eclipse Adoptium\"\nJAVA_VERSION=\"17.0.18\"\nJAVA_VERSION_DATE=\"2026-01-20\"\n");
                Assert.AreEqual("17.0.18", JdkRelease.ReadVersion(tmp.Root));
            }
        }

        [Test] public void JdkRelease_MissingFileOrKey_ReturnsNull()
        {
            using (var tmp = new TempDirectory())
            {
                Assert.IsNull(JdkRelease.ReadVersion(tmp.Root));
                tmp.File("release", "IMPLEMENTOR=\"x\"\n");
                Assert.IsNull(JdkRelease.ReadVersion(tmp.Root));
            }
            Assert.IsNull(JdkRelease.ReadVersion(null));
        }

        [TestCase("17.0.18", 17)]
        [TestCase("21", 21)]
        [TestCase("21.0.2", 21)]
        [TestCase("1.8.0_292", 8)]
        [TestCase("11.0.20", 11)]
        [TestCase("garbage", -1)]
        [TestCase("", -1)]
        [TestCase(null, -1)]
        public void JdkRelease_ParseMajor(string version, int expected)
        {
            Assert.AreEqual(expected, JdkRelease.ParseMajor(version));
        }

        // --- NdkProperties ---

        [Test] public void NdkProperties_ReadsRevision()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("source.properties", "Pkg.Desc = Android NDK\nPkg.Revision = 27.2.12479018\nPkg.BaseRevision = 27.2.12479018\n");
                Assert.AreEqual("27.2.12479018", NdkProperties.ReadRevision(tmp.Root));
            }
        }

        [Test] public void NdkProperties_MissingFile_ReturnsNull()
        {
            using (var tmp = new TempDirectory()) Assert.IsNull(NdkProperties.ReadRevision(tmp.Root));
            Assert.IsNull(NdkProperties.ReadRevision(null));
        }

        // --- GradleVersion ---

        [Test] public void GradleVersion_ReadsLauncherJarName()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("lib/ant-launcher-1.10.15.jar");
                tmp.File("lib/gradle-launcher-9.3.1.jar");
                Assert.AreEqual("9.3.1", GradleVersion.FromLauncherJar(tmp.Root));
            }
        }

        [Test] public void GradleVersion_NoLauncherJar_ReturnsNull()
        {
            using (var tmp = new TempDirectory())
            {
                Assert.IsNull(GradleVersion.FromLauncherJar(tmp.Root));
                tmp.File("lib/ant-launcher-1.10.15.jar");
                Assert.IsNull(GradleVersion.FromLauncherJar(tmp.Root));
            }
            Assert.IsNull(GradleVersion.FromLauncherJar(null));
        }

        // --- AndroidSdkLayout ---

        [Test] public void SdkLayout_ListsPlatformApis_IncludingMinorSuffixes_AndIgnoringPreviews()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.Dir("platforms/android-34");
                tmp.Dir("platforms/android-36");
                tmp.Dir("platforms/android-37.0");
                tmp.Dir("platforms/android-VanillaIceCream");
                CollectionAssert.AreEqual(new[] { 34, 36, 37 }, AndroidSdkLayout.InstalledApis(tmp.Root));
                Assert.AreEqual(37, AndroidSdkLayout.HighestApi(tmp.Root));
                Assert.IsTrue(AndroidSdkLayout.HasPlatform(tmp.Root, 36));
                Assert.IsFalse(AndroidSdkLayout.HasPlatform(tmp.Root, 35));
            }
        }

        [Test] public void SdkLayout_NoPlatforms_HighestIsMinusOne()
        {
            using (var tmp = new TempDirectory())
            {
                Assert.IsEmpty(AndroidSdkLayout.InstalledApis(tmp.Root));
                Assert.AreEqual(-1, AndroidSdkLayout.HighestApi(tmp.Root));
            }
        }

        [Test] public void SdkLayout_CommandLineTools_AcceptsVersionFolderOrLegacyToolsWithContent()
        {
            using (var tmp = new TempDirectory())
            {
                Assert.IsFalse(AndroidSdkLayout.HasCommandLineTools(tmp.Root));
                tmp.Dir("tools"); // an empty legacy folder does not count
                Assert.IsFalse(AndroidSdkLayout.HasCommandLineTools(tmp.Root));
                tmp.Dir("cmdline-tools/16.0");
                Assert.IsTrue(AndroidSdkLayout.HasCommandLineTools(tmp.Root));
            }
            using (var tmp = new TempDirectory())
            {
                tmp.File("tools/android.bat");
                Assert.IsTrue(AndroidSdkLayout.HasCommandLineTools(tmp.Root));
            }
        }

        [Test] public void SdkLayout_PlatformToolsAndBuildTools()
        {
            using (var tmp = new TempDirectory())
            {
                Assert.IsFalse(AndroidSdkLayout.HasPlatformTools(tmp.Root));
                tmp.Dir("platform-tools");
                tmp.Dir("build-tools/36.0.0");
                tmp.Dir("build-tools/34.0.0");
                Assert.IsTrue(AndroidSdkLayout.HasPlatformTools(tmp.Root));
                CollectionAssert.AreEqual(new[] { "34.0.0", "36.0.0" }, AndroidSdkLayout.InstalledBuildTools(tmp.Root));
            }
        }

        [Test] public void SdkLayout_NullOrMissingPath_IsSafe()
        {
            Assert.IsEmpty(AndroidSdkLayout.InstalledApis(null));
            Assert.IsEmpty(AndroidSdkLayout.InstalledBuildTools("Z:/does/not/exist"));
            Assert.IsFalse(AndroidSdkLayout.HasPlatformTools(null));
            Assert.IsFalse(AndroidSdkLayout.HasCommandLineTools(null));
        }
    }
}
