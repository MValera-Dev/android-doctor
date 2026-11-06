using AndroidDoctor.Environment;

namespace AndroidDoctor.Project
{
    internal static class TargetApiResolver
    {
        /// <summary>
        /// The API level a build would really target. A configured level wins; "Auto" (0 or less) means the
        /// highest platform installed in the SDK. Returns -1 when it cannot be known.
        /// </summary>
        public static int Effective(int configured, ToolPath sdk)
        {
            if (configured > 0) return configured;
            if (sdk == null || !sdk.Exists) return -1;
            return AndroidSdkLayout.HighestApi(sdk.Path);
        }

        /// <summary>"Auto" for 0 or less, otherwise the number.</summary>
        public static string Format(int level) => level <= 0 ? "Auto" : level.ToString();
    }
}
