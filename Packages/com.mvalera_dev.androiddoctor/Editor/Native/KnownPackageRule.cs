using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidDoctor.Native
{
    /// <summary>
    /// Base for rules driven by one entry of play.json "knownPackages": a package whose old versions are known
    /// to ship native libraries that are not 16 KB aligned. Severity, minimum version and reason are data.
    /// </summary>
    public abstract class KnownPackageRule : IRule
    {
        public abstract string Id { get; }
        public Category Category => Category.Native;
        protected abstract string PackageName { get; }

        public IEnumerable<Finding> Run(RuleContext ctx) => Evaluate(ctx);

        internal IEnumerable<Finding> Evaluate(RuleContext ctx)
        {
            if (ctx.Play == null) yield break;
            var known = ctx.Play.knownPackages.FirstOrDefault(k => k != null && k.name == PackageName);
            if (known == null || string.IsNullOrEmpty(known.minVersion)) yield break;

            var installed = ctx.Packages.FirstOrDefault(p => p.Name == PackageName);
            if (installed == null || !VersionCompare.IsLess(installed.Version, known.minVersion)) yield break;

            var evidence = new List<string>
            {
                Messages.Get("known.evidence.installed", installed.Name, installed.Version),
                Messages.Get("known.evidence.minimum", known.minVersion),
            };
            if (!string.IsNullOrEmpty(known.reason)) evidence.Add(Messages.Get("known.evidence.reason", known.reason));

            yield return new Finding(Id, Category.Native, ParseSeverity(known.severity),
                Messages.Get(Id + ".title", installed.Version, known.minVersion),
                Messages.Get(Id + ".detail", installed.Name), evidence);
        }

        static Severity ParseSeverity(string text)
        {
            Severity parsed;
            return Enum.TryParse(text, true, out parsed) ? parsed : Severity.Warning;
        }
    }

    /// <summary>NAT004: a Burst version old enough to ship libburst_generated.so without 16 KB alignment.</summary>
    public sealed class BurstVersionRule : KnownPackageRule
    {
        public override string Id => "NAT004";
        protected override string PackageName => "com.unity.burst";
    }

    /// <summary>NAT005: an old Google Play App Bundle package (a community-reported, unofficial datum).</summary>
    public sealed class AppBundlePackageRule : KnownPackageRule
    {
        public override string Id => "NAT005";
        protected override string PackageName => "com.google.android.appbundle";
    }
}
