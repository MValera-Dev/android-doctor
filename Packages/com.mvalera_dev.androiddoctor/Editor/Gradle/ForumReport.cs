using System.Text;
using UnityEngine;

namespace AndroidDoctor.Gradle
{
    /// <summary>The text to paste into a forum when asking for help: the versions and the original failure block.</summary>
    public static class ForumReport
    {
        public static string Build(GradleError error, string unityVersion, string operatingSystem)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Unity " + unityVersion + " · " + operatingSystem);
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine(error == null ? "" : error.Block);
            sb.AppendLine("```");
            return sb.ToString();
        }

        public static string Build(GradleError error)
        {
            return Build(error, Application.unityVersion, SystemInfo.operatingSystem);
        }
    }
}
