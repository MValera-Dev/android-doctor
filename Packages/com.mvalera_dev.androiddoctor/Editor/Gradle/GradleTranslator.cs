using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidDoctor.Gradle
{
    /// <summary>Turns a failed Gradle build into findings: the probable cause, or GRD000 when nothing is recognised.</summary>
    public static class GradleTranslator
    {
        public const string UnknownId = "GRD000";
        const int MaxEvidenceLength = 240;
        const int UnknownEvidenceLines = 3;

        /// <param name="getEnvironmentVariable">Reads the environment; defaults to the real process environment.</param>
        public static List<Finding> Translate(GradleError error, Func<string, string> getEnvironmentVariable = null, ProjectIndex project = null)
        {
            var findings = new List<Finding>();
            if (error == null) return findings;
            getEnvironmentVariable = getEnvironmentVariable ?? System.Environment.GetEnvironmentVariable;
            project = project ?? new ProjectIndex();

            foreach (var pattern in GradlePatterns.All.OrderBy(p => p.Priority).ThenBy(p => p.Id, StringComparer.Ordinal))
            {
                if (pattern.RequiredEnvVar != null && string.IsNullOrEmpty(getEnvironmentVariable(pattern.RequiredEnvVar))) continue;
                var m = pattern.Rx.Match(error.Block ?? "");
                if (!m.Success) continue;

                // Captured groups fill the {0}, {1}... of the texts.
                var args = new object[m.Groups.Count - 1];
                for (int i = 1; i < m.Groups.Count; i++) args[i - 1] = m.Groups[i].Value.Trim();

                string command = pattern.CommandTemplate == null ? null : string.Format(pattern.CommandTemplate, args);
                findings.Add(new Finding(pattern.Id, Category.Gradle, Severity.Error,
                    Messages.Get(pattern.Id + ".title", args), Messages.Get(pattern.Id + ".detail", args),
                    new[] { Messages.Get("gradle.evidence", Shorten(FirstLine(m.Value))) }, command: command,
                    actions: GradleFixes.For(pattern.Id, args, project)));
            }

            if (findings.Count == 0) findings.Add(Unknown(error, project));
            return findings;
        }

        static Finding Unknown(GradleError error, ProjectIndex project)
        {
            var lines = (error.WhatWentWrong ?? "").Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Trim().Length > 0)
                .Take(UnknownEvidenceLines).Select(Shorten).ToList();
            return new Finding(UnknownId, Category.Gradle, Severity.Error,
                Messages.Get("GRD000.title"), Messages.Get("GRD000.detail"), lines,
                actions: GradleFixes.For(UnknownId, new object[0], project));
        }

        static string FirstLine(string text)
        {
            int i = text.IndexOf('\n');
            return (i < 0 ? text : text.Substring(0, i)).TrimEnd('\r').Trim();
        }

        static string Shorten(string text)
        {
            return text.Length <= MaxEvidenceLength ? text : text.Substring(0, MaxEvidenceLength) + "...";
        }
    }
}
