namespace AndroidDoctor
{
    /// <summary>
    /// Stable ids of the fixes. Rules put these in Finding.FixId and each fix returns the same constant
    /// from IFix.Id, so a rule and its fix cannot drift apart.
    /// </summary>
    internal static class FixIds
    {
        public const string TargetApi = "FIX_TARGET_API";
        public const string MinApi = "FIX_MIN_API";
        public const string Arm64 = "FIX_ARM64";
        public const string OpenExternalTools = "FIX_OPEN_EXTERNAL_TOOLS";
        public const string SelectAsset = "FIX_SELECT_ASSET";

        public static readonly string[] All = { TargetApi, MinApi, Arm64, OpenExternalTools, SelectAsset };
    }
}
