using System;
using UnityEditor;

namespace AndroidDoctor.Project
{
    /// <summary>Raises the minimum API level to the minimum this Unity version requires.</summary>
    public sealed class SetMinApiFix : IActionFix
    {
        public string Id => FixIds.MinApi;

        static int RequiredMin(RuleContext ctx)
        {
            var row = ctx.Compat == null ? null : ctx.Compat.Find(ctx.Unity);
            return row == null ? -1 : row.minSdk;
        }

        /// <summary>The level a library asks for (the action argument), else the minimum Unity requires.</summary>
        static int Level(FixAction action)
        {
            int fromArgument;
            if (action != null && int.TryParse(action.Arg, out fromArgument) && fromArgument > 0) return fromArgument;
            return RequiredMin(RuleContext.Create());
        }

        public string Describe(Finding f) => Describe(f, null);

        public string Describe(Finding f, FixAction action)
        {
            return Messages.Get("fix.minApi.describe", (int)PlayerSettings.Android.minSdkVersion, Level(action));
        }

        public bool Apply(Finding f, out string message) => Apply(f, null, out message);

        public bool Apply(Finding f, FixAction action, out string message)
        {
            int required = Level(action);
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
