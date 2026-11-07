using System.Text.RegularExpressions;

namespace AndroidDoctor.Gradle
{
    public sealed class GradlePattern
    {
        public string Id;              // "GRD003"
        public int Priority;           // lower is shown first
        public Regex Rx;
        public string RequiredEnvVar;  // the pattern only applies when this environment variable is set
        public string CommandTemplate; // optional copyable command; {0}, {1}... are the captured groups

        public GradlePattern(string id, int priority, string regex, string requiredEnvVar = null, string commandTemplate = null)
        {
            Id = id; Priority = priority; RequiredEnvVar = requiredEnvVar; CommandTemplate = commandTemplate;
            Rx = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }
    }

    /// <summary>
    /// Every pattern here was checked against a real Gradle log in Tests/Editor/Fixtures/Gradle/ (Unity 6000.6.4f1,
    /// Gradle 9.3.1, Android Gradle plugin 9.1.1). A pattern with no real log is not published: GRD001, GRD002,
    /// GRD012, GRD013 and GRD017 of the design document are missing for that reason.
    /// </summary>
    public static class GradlePatterns
    {
        public static readonly GradlePattern[] All =
        {
            // Failed to apply the Android plugin while ANDROID_SDK_HOME is set (explains the root cause, so it goes first).
            new GradlePattern("GRD015", 5, @"Failed to apply plugin 'com\.android\.[\w.]+'", requiredEnvVar: "ANDROID_SDK_HOME"),
            // A setting the Android Gradle plugin removed (seen with android.enableDexingArtifactTransform on Unity 6000.6).
            new GradlePattern("GRD021", 10, "The option '(android[.][A-Za-z0-9_.]+)' is deprecated[.](?s:.*?)removed in version ([0-9.]+) of the Android Gradle plugin"),
            new GradlePattern("GRD003", 10, @"Failed to find Platform SDK with path: platforms;android-(\d+(?:\.\d+)?)",
                commandTemplate: "sdkmanager \"platforms;android-{0}\""),
            new GradlePattern("GRD004", 10, @"Failed to find Build Tools revision ([\d.]+)",
                commandTemplate: "sdkmanager \"build-tools;{0}\""),
            new GradlePattern("GRD005", 10, @"\[CXX1100\] android\.ndkVersion is \[([^\]]+)\] but android\.ndkPath .* refers to a different version \[([^\]]+)\]"),
            new GradlePattern("GRD014", 15, @"Could not compile build file '([^']+)'"),
            new GradlePattern("GRD006", 20, @"Duplicate class ([\w.$]+) found in modules ([^\r\n]+)"),
            new GradlePattern("GRD007", 20, @"android:exported needs to be explicitly specified for element <(\w+)#([\w.$]+)>"),
            new GradlePattern("GRD008", 20, @"(\d+) files found with path '(lib/[^']+\.so)'"),
            new GradlePattern("GRD011", 20, @"minSdkVersion (\d+) cannot be smaller than version (\d+) declared in library \[([^\]]+)\]"),
            new GradlePattern("GRD016", 20, @"Failed to read key (\S+) from store"),
            new GradlePattern("GRD019", 20, @"Attribute (\S+) value=\(([^)]*)\) from [^\n]*\n\s*is also present at \[([^\]]+)\]"),
            new GradlePattern("GRD020", 20, @"([^\\/\r\n]*\.java):(\d+): error: ([^\r\n]+)"),
            new GradlePattern("GRD018", 25, @"Could not find ([\w.\-]+:[\w.\-]+:[\w.\-]*[\w\-])"),
            new GradlePattern("GRD009", 30, @"java\.lang\.OutOfMemoryError"),
            new GradlePattern("GRD010", 40, @"Could not (?:GET|HEAD) '(https?://[^'\s]+)'"),
        };
    }
}
