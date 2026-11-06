using System;
using UnityEngine;

namespace AndroidDoctor
{
    [Serializable]
    public sealed class KnownPackage
    {
        public string name;
        public string minVersion;
        public string severity;   // "Error" | "Warning"
        public string reason;
    }

    [Serializable]
    public sealed class PlayRequirements
    {
        public string asOf;
        public int minTargetSdk = -1;          // -1 = unknown
        public string minTargetSdkDeadline;    // ISO date
        public bool require64Bit;
        public bool require16KB;
        public KnownPackage[] knownPackages = new KnownPackage[0];

        /// <summary>Loads Editor/Data/play.json from the package. Null if it cannot be read.</summary>
        public static PlayRequirements Load() => Parse(DataFiles.ReadText("play.json"));

        /// <summary>Parses play.json text. Null if the text is empty or not valid JSON.</summary>
        public static PlayRequirements Parse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var play = JsonUtility.FromJson<PlayRequirements>(json);
                if (play != null && play.knownPackages == null) play.knownPackages = new KnownPackage[0];
                return play;
            }
            catch (ArgumentException) { return null; }
        }
    }
}
