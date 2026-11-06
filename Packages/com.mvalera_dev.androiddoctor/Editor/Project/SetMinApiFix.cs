using System;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>Raises the minimum API level to the minimum this Unity version requires.</summary>
    public sealed class SetMinApiFix : IFix
    {
        public string Id => FixIds.MinApi;

        static int RequiredMin(RuleContext ctx)
        {
            var row = ctx.Compat == null ? null : ctx.Compat.Find(ctx.Unity);
            return row == null ? -1 : row.minSdk;
        }

        public string Describe(Finding f)
        {
            return Messages.Get("fix.minApi.describe",
                (int)PlayerSettings.Android.minSdkVersion, RequiredMin(RuleContext.Create()));
        }

        public bool Apply(Finding f, out string message)
        {
            int required = RequiredMin(RuleContext.Create());
            if (required <= 0) { message = Messages.Get("fix.minApi.unknown"); return false; }
            if (!Enum.IsDefined(typeof(AndroidSdkVersions), required))
            {
                message = Messages.Get("fix.apiLevel.unsupported", required);
                return false;
            }
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)required;
            message = Messages.Get("fix.minApi.done", required);
            return true;
        }
    }
}
