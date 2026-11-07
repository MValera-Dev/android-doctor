using System;

namespace AndroidDoctor.Environment
{
    /// <summary>
    /// Removes an environment variable from the Unity Editor process. Gradle is started by the Editor and inherits its
    /// environment, so this fixes the build for as long as Unity stays open. The variable itself is not touched on disk.
    /// </summary>
    public sealed class ClearEnvironmentVariableFix : IActionFix
    {
        public string Id => FixIds.ClearEnvironmentVariable;

        public string Describe(Finding f) => Describe(f, null);

        public string Describe(Finding f, FixAction action)
        {
            string name = action?.Arg;
            if (string.IsNullOrEmpty(name)) return Messages.Get("fix.env.unknown");
            return Messages.Get("fix.env.describe", name, System.Environment.GetEnvironmentVariable(name));
        }

        public bool Apply(Finding f, out string message) => Apply(f, null, out message);

        public bool Apply(Finding f, FixAction action, out string message)
        {
            string name = action?.Arg;
            if (string.IsNullOrEmpty(name)) { message = Messages.Get("fix.env.unknown"); return false; }
            if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(name)))
            {
                message = Messages.Get("fix.env.notSet", name);
                return false;
            }
            System.Environment.SetEnvironmentVariable(name, null); // process scope only
            message = Messages.Get("fix.env.done", name);
            return true;
        }
    }
}
