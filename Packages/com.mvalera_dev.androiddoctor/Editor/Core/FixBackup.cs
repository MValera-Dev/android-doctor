using System;
using System.IO;
using UnityEngine;

namespace AndroidDoctor
{
    /// <summary>
    /// Keeps a copy of a project file before a fix edits or removes it. Copies live in Library/AndroidDoctor/Backups,
    /// next to the other things Unity can regenerate, in one folder per run.
    /// </summary>
    internal static class FixBackup
    {
        static string _runFolder;
        static DateTime _runStarted;

        /// <summary>The folder of the current minute; a fix that touches several files keeps them together.</summary>
        public static string RunFolder
        {
            get
            {
                if (_runFolder == null || (DateTime.Now - _runStarted).TotalMinutes > 1)
                {
                    _runStarted = DateTime.Now;
                    _runFolder = Path.Combine(ProjectPath, "Library", "AndroidDoctor", "Backups", _runStarted.ToString("yyyyMMdd-HHmmss"));
                }
                return _runFolder;
            }
        }

        public static string ProjectPath => Path.GetDirectoryName(Application.dataPath);

        /// <summary>Copies a project-relative file into the backup folder. Returns where the copy is.</summary>
        public static string Save(string projectRelativePath)
        {
            string source = Path.Combine(ProjectPath, projectRelativePath);
            string destination = Path.Combine(RunFolder, projectRelativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(source, destination, true);
            return destination;
        }
    }
}
