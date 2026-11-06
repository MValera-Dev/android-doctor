using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using AndroidDoctor.Native;

namespace AndroidDoctor.Build
{
    /// <summary>Reads the native libraries of an APK or AAB that was already built, from the zip alone.</summary>
    public static class ArtifactScanner
    {
        public const string BuildOutputOwner = "(build output)";

        // APK: lib/<abi>/x.so    AAB: base/lib/<abi>/x.so (or <module>/lib/<abi>/x.so)
        static readonly Regex LibEntry = new Regex(@"^(?:[^/]+/)?lib/(arm64-v8a|x86_64|armeabi-v7a|x86)/([^/]+\.so)$");

        /// <summary>Scans the file. On failure returns false with the reason and never throws.</summary>
        public static bool TryScan(string apkOrAabPath, out List<NativeLib> libs, out string error)
        {
            libs = new List<NativeLib>(); error = null;
            try
            {
                using (var fs = new FileStream(apkOrAabPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    foreach (var entry in zip.Entries)
                    {
                        var m = LibEntry.Match(entry.FullName);
                        if (!m.Success) continue;
                        var lib = new NativeLib
                        {
                            Name = m.Groups[2].Value,
                            Abi = m.Groups[1].Value,
                            Location = apkOrAabPath.Replace('\\', '/') + "!/" + entry.FullName,
                            Owner = BuildOutputOwner,
                        };
                        string readError;
                        lib.Elf = ZipElfReader.Read(entry, out readError);
                        if (lib.Elf == null) lib.ReadError = readError;
                        libs.Add(lib);
                    }
                }
                return true;
            }
            catch (InvalidDataException e) { error = e.Message; }
            catch (IOException e) { error = e.Message; }
            catch (UnauthorizedAccessException e) { error = e.Message; }
            libs = new List<NativeLib>();
            return false;
        }
    }
}
