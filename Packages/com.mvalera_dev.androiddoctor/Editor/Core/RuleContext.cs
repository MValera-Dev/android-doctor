using System;
using System.Collections.Generic;
using System.IO;
using AndroidDoctor.Environment;
using UnityEngine;

namespace AndroidDoctor
{
    public sealed class PackageRef
    {
        public string Name, Version, ResolvedPath;
    }

    /// <summary>Everything a rule needs to know, computed once per run.</summary>
    public sealed class RuleContext
    {
        public UnityVersion Unity;
        public AndroidTools Tools;
        public CompatTable Compat;      // Editor/Data/compat.json (null if unreadable)
        public PlayRequirements Play;   // Editor/Data/play.json (null if unreadable)
        public string ProjectPath;      // folder that contains Assets/
        public string AssetsPath;
        public string ArtifactPath;     // the APK/AAB the build checks should open; null when there is none
        public List<PackageRef> Packages = new List<PackageRef>();

        readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        public T GetOrCompute<T>(string key, Func<T> factory)
        {
            if (_cache.TryGetValue(key, out var v)) return (T)v;
            var created = factory();
            _cache[key] = created;
            return created;
        }

        public static RuleContext Create()
        {
            var ctx = new RuleContext
            {
                Unity = UnityVersion.Parse(Application.unityVersion),
                Tools = AndroidToolsReader.Read(),
                ProjectPath = Directory.GetParent(Application.dataPath).FullName,
                AssetsPath = Application.dataPath,
                ArtifactPath = AndroidDoctor.Build.BuildArtifactState.Get(),
                Compat = CompatTable.Load(),
                Play = PlayRequirements.Load(),
            };
            foreach (var p in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
                ctx.Packages.Add(new PackageRef { Name = p.name, Version = p.version, ResolvedPath = p.resolvedPath });
            return ctx;
        }
    }
}
