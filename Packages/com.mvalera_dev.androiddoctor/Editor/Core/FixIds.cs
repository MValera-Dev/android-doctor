namespace AndroidDoctor
{
    /// <summary>
    /// Stable ids of the fixes. Rules put these in a FixAction and each fix returns the same constant
    /// from IFix.Id, so a rule and its fix cannot drift apart.
    /// </summary>
    internal static class FixIds
    {
        public const string TargetApi = "FIX_TARGET_API";
        public const string MinApi = "FIX_MIN_API";
        public const string Arm64 = "FIX_ARM64";
        public const string OpenExternalTools = "FIX_OPEN_EXTERNAL_TOOLS";
        public const string SelectAsset = "FIX_SELECT_ASSET";
        public const string UnityTools = "FIX_UNITY_TOOLS";
        public const string Heap = "FIX_HEAP";
        public const string ClearEnvironmentVariable = "FIX_CLEAR_ENV";
        public const string AppBundle = "FIX_APP_BUNDLE";
        public const string UpdatePackage = "FIX_UPDATE_PACKAGE";
        public const string OpenPackage = "FIX_OPEN_PACKAGE";
        public const string DisablePlugin = "FIX_DISABLE_PLUGIN";
        public const string Exported = "FIX_EXPORTED";
        public const string ResetTemplates = "FIX_RESET_TEMPLATES";
        public const string GradleProperties = "FIX_GRADLE_PROPERTIES";
        public const string CleanCache = "FIX_CLEAN_CACHE";
        public const string OpenPlayerSettings = "FIX_OPEN_PLAYER_SETTINGS";
        public const string ResetResolver = "FIX_RESET_RESOLVER";

        public static readonly string[] All =
        {
            TargetApi, MinApi, Arm64, OpenExternalTools, SelectAsset, UnityTools, Heap, ClearEnvironmentVariable,
            AppBundle, UpdatePackage, OpenPackage, DisablePlugin, Exported, ResetTemplates, GradleProperties,
            CleanCache, OpenPlayerSettings, ResetResolver,
        };

        /// <summary>False for fixes that only open a window or select something; they never belong in "Fix all".</summary>
        public static bool ChangesSettings(string fixId)
        {
            return fixId != OpenExternalTools && fixId != SelectAsset && fixId != OpenPackage
                && fixId != OpenPlayerSettings;
        }
    }
}
