using System.IO;
using System.Linq;
using NUnit.Framework;
using AndroidDoctor.Project;

namespace AndroidDoctor.Tests
{
    public class ProjectUtilitiesTests
    {
        static string Fixture(string name)
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TempDirectory).Assembly);
            return File.ReadAllText(Path.Combine(package.resolvedPath, "Tests", "Editor", "Fixtures", "Manifests", name));
        }

        // --- ArtifactName ---

        [TestCase("play-services-basement-18.3.0.aar", "play-services-basement", "18.3.0")]
        [TestCase("commons-io-2.11.0.jar", "commons-io", "2.11.0")]
        [TestCase("foo-2.aar", "foo", "2")]
        [TestCase("foo-1.2.3-beta.1.aar", "foo", "1.2.3-beta.1")]
        [TestCase("foo-1.0.0-SNAPSHOT.jar", "foo", "1.0.0-SNAPSHOT")]
        [TestCase("FOO-1.0.AAR", "FOO", "1.0")]
        [TestCase("Assets/Plugins/Android/androidx-core-1.12.0.aar", "androidx-core", "1.12.0")]
        public void ArtifactName_SplitsNameAndVersion(string file, string artifact, string version)
        {
            string a, v;
            Assert.IsTrue(ArtifactName.TryParse(file, out a, out v));
            Assert.AreEqual(artifact, a);
            Assert.AreEqual(version, v);
        }

        [TestCase("foo.aar", "foo")]
        [TestCase("foo-release.aar", "foo-release")]
        [TestCase("lib-v2.jar", "lib-v2")]
        public void ArtifactName_NameWithoutVersion_HasNullVersion(string file, string artifact)
        {
            string a, v;
            Assert.IsTrue(ArtifactName.TryParse(file, out a, out v));
            Assert.AreEqual(artifact, a);
            Assert.IsNull(v);
        }

        [TestCase("libfoo.so")]
        [TestCase("readme.md")]
        [TestCase(".aar")]
        [TestCase("")]
        [TestCase(null)]
        public void ArtifactName_NotALibrary_ReturnsFalse(string file)
        {
            string a, v;
            Assert.IsFalse(ArtifactName.TryParse(file, out a, out v));
        }

        // --- ManifestExportedChecker ---

        [Test] public void Manifest_FindsComponentsWithIntentFilterAndNoExported()
        {
            var missing = ManifestExportedChecker.FindMissing(Fixture("missing_exported.xml"));

            Assert.AreEqual(2, missing.Count);
            Assert.AreEqual("activity", missing[0].Tag);
            Assert.AreEqual("com.example.MainActivity", missing[0].Name);
            Assert.AreEqual(4, missing[0].Line);
            Assert.AreEqual("receiver", missing[1].Tag);
            Assert.AreEqual("com.example.BootReceiver", missing[1].Name);
        }

        [Test] public void Manifest_ExplicitExported_IsFine()
        {
            Assert.IsEmpty(ManifestExportedChecker.FindMissing(Fixture("all_exported.xml")));
        }

        [Test] public void Manifest_NoIntentFilter_IsFine()
        {
            Assert.IsEmpty(ManifestExportedChecker.FindMissing(Fixture("no_intent_filter.xml")));
        }

        [Test] public void Manifest_MalformedOrEmptyText_ReturnsEmptyWithoutThrowing()
        {
            Assert.IsEmpty(ManifestExportedChecker.FindMissing(Fixture("malformed.xml")));
            Assert.IsEmpty(ManifestExportedChecker.FindMissing(""));
            Assert.IsEmpty(ManifestExportedChecker.FindMissing(null));
        }

        // --- ProjectFileWalker ---

        [Test] public void Walker_FindsFilesRecursively_AndSkipsHiddenAndUnityIgnoredFolders()
        {
            using (var tmp = new TempDirectory())
            {
                tmp.File("Assets/a.aar");
                tmp.File("Assets/Plugins/b.aar");
                tmp.File("Assets/.hidden/c.aar");
                tmp.File("Assets/Samples~/d.aar");
                tmp.File("Assets/Library/e.aar");
                tmp.File("Assets/node_modules/f.aar");
                tmp.File("Assets/obj/g.aar");
                tmp.File("Assets/Plugins/readme.txt");

                var found = ProjectFileWalker.FindFiles(tmp.Dir("Assets"), f => f.EndsWith(".aar"))
                    .Select(f => Path.GetFileName(f)).OrderBy(n => n).ToArray();

                CollectionAssert.AreEqual(new[] { "a.aar", "b.aar" }, found);
            }
        }

        [Test] public void Walker_MissingOrEmptyRoot_ReturnsEmpty()
        {
            Assert.IsEmpty(ProjectFileWalker.FindFiles("Z:/does/not/exist", f => true));
            Assert.IsEmpty(ProjectFileWalker.FindFiles(null, f => true));
        }

        [Test] public void ToProjectRelative_StripsProjectFolderAndUsesForwardSlashes()
        {
            Assert.AreEqual("Assets/Plugins/a.aar",
                ProjectFileWalker.ToProjectRelative("C:\\Dev\\Proj", "C:\\Dev\\Proj\\Assets\\Plugins\\a.aar"));
            Assert.AreEqual("Assets/a.aar",
                ProjectFileWalker.ToProjectRelative("C:/Dev/Proj/", "c:/dev/proj/Assets/a.aar"));
            Assert.AreEqual("D:/Other/a.aar", ProjectFileWalker.ToProjectRelative("C:/Dev/Proj", "D:\\Other\\a.aar"));
        }
    }
}
