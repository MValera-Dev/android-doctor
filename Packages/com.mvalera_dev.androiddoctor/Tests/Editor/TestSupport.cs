using System;
using System.IO;
using AndroidDoctor.Environment;

namespace AndroidDoctor.Tests
{
    /// <summary>A throwaway folder under the system temp path, deleted on Dispose.</summary>
    public sealed class TempDirectory : IDisposable
    {
        public string Root { get; }

        public TempDirectory()
        {
            Root = Path.Combine(Path.GetTempPath(), "androiddoctor-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        /// <summary>Creates (if needed) and returns the folder at a path relative to the root.</summary>
        public string Dir(string relative)
        {
            string full = Path.Combine(Root, relative);
            Directory.CreateDirectory(full);
            return full;
        }

        /// <summary>Writes a text file at a path relative to the root and returns its full path.</summary>
        public string File(string relative, string content = "")
        {
            string full = Path.Combine(Root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            System.IO.File.WriteAllText(full, content);
            return full;
        }

        public void Dispose()
        {
            try { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    public static class TestContexts
    {
        /// <summary>compat.json for a Unity 6000.x editor: JDK 17, NDK 27.2, build-tools 36, Gradle 9.3.1.</summary>
        public const string Compat6000 = @"{ ""asOf"": ""test"", ""unity"": [
            { ""match"": ""6000"", ""jdk"": 17, ""ndk"": ""27.2.12479018"", ""buildTools"": ""36.0.0"", ""gradle"": ""9.3.1"", ""agp"": ""9.1.1"", ""minSdk"": 26 }
        ] }";

        /// <summary>A hand-built context: Unity 6000.6.4f1, empty tools, the compat table above, no Play data.</summary>
        public static RuleContext Create(string compatJson = Compat6000)
        {
            return new RuleContext
            {
                Unity = UnityVersion.Parse("6000.6.4f1"),
                Tools = new AndroidTools(),
                Compat = CompatTable.Parse(compatJson),
            };
        }

        public static ToolPath Tool(string path, ToolSource source)
        {
            return new ToolPath { Path = path, Source = source };
        }
    }
}
