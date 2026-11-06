using System;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>Sets the target API level to the Google Play minimum.</summary>
    public sealed class SetTargetApiFix : IFix
    {
        public string Id => FixIds.TargetApi;

        public string Describe(Finding f)
        {
            var ctx = RuleContext.Create();
            int minimum = ctx.Play == null ? -1 : ctx.Play.minTargetSdk;
            return Messages.Get("fix.targetApi.describe",
                TargetApiResolver.Format((int)PlayerSettings.Android.targetSdkVersion), minimum);
        }

        public bool Apply(Finding f, out string message)
        {
            var ctx = RuleContext.Create();
            int minimum = ctx.Play == null ? -1 : ctx.Play.minTargetSdk;
            if (minimum <= 0) { message = Messages.Get("fix.targetApi.unknown"); return false; }
            // A cast always compiles, so check that this Unity version actually has that level.
            if (!Enum.IsDefined(typeof(AndroidSdkVersions), minimum))
            {
                message = Messages.Get("fix.apiLevel.unsupported", minimum);
                return false;
            }
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)minimum;
            message = Messages.Get("fix.targetApi.done", minimum);
            return true;
        }
    }
}
