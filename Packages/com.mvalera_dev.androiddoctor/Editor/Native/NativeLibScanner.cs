using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using AndroidDoctor.Project;

namespace AndroidDoctor.Native
{
    /// <summary>A folder to scan and how its files are named as assets.</summary>
    public sealed class ScanRoot
    {
        public string Path;          // folder on disk
        public string AssetPrefix;   // "Assets" or "Packages/com.vendor.sdk"
        public bool IsPackage;       // true: every library in it is owned by AssetPrefix
    }

    /// <summary>Finds the native libraries (.so files and the .so files inside .aar files) of a project.</summary>
    public static class NativeLibScanner
    {
        public const string CacheKey = "nativeLibs";

        static readonly string[] Abis = { "arm64-v8a", "armeabi-v7a", "x86_64", "x86", "armeabi" };
        static readonly HashSet<string> GenericFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Plugins", "Android", "libs", "lib", "jni",
        };

        /// <summary>The libraries of this project, scanned once per run and shared by NAT001, NAT002 and NAT003.</summary>
        public static List<NativeLib> Get(RuleContext ctx)
        {
            return ctx.GetOrCompute(CacheKey, () => Scan(Roots(ctx), IsAndroidPlugin));
        }

        static List<ScanRoot> Roots(RuleContext ctx)
        {
            var roots = new List<ScanRoot>();
            if (!string.IsNullOrEmpty(ctx.AssetsPath))
                roots.Add(new ScanRoot { Path = ctx.AssetsPath, AssetPrefix = "Assets" });
            foreach (var p in ctx.Packages)
            {
                if (string.IsNullOrEmpty(p.ResolvedPath) || p.Name.StartsWith("com.unity.modules.")) continue;
                roots.Add(new ScanRoot { Path = p.ResolvedPath, AssetPrefix = "Packages/" + p.Name, IsPackage = true });
            }
            return roots;
        }

        // A .so outside Android's plugin settings (a desktop Linux plugin in Plugins/x86_64, say) is not an
        // Android library. Asking the importer avoids the most likely false positive.
        static bool IsAndroidPlugin(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as PluginImporter;
            return importer != null && importer.GetCompatibleWithPlatform(BuildTarget.Android);
        }

        /// <param name="isAndroidAsset">Whether an asset path is compiled into Android builds.</param>
        public static List<NativeLib> Scan(IEnumerable<ScanRoot> roots, Func<string, bool> isAndroidAsset)
        {
            var libs = new List<NativeLib>();
            foreach (var root in roots)
            {
                var files = ProjectFileWalker.FindFiles(root.Path, f =>
                {
                    string ext = Path.GetExtension(f);
                    return ext.Equals(".so", StringComparison.OrdinalIgnoreCase) || ext.Equals(".aar", StringComparison.OrdinalIgnoreCase);
                });
                foreach (string file in files)
                {
                    string assetPath = root.AssetPrefix + "/" + RelativeTo(root.Path, file);
                    if (!isAndroidAsset(assetPath)) continue;
                    if (file.EndsWith(".aar", StringComparison.OrdinalIgnoreCase)) ScanAar(file, assetPath, libs);
                    else libs.Add(ScanSo(file, assetPath, root));
                }
            }
            return libs;
        }

        static NativeLib ScanSo(string file, string assetPath, ScanRoot root)
        {
            var lib = new NativeLib
            {
                Name = Path.GetFileName(file),
                Location = assetPath,
                Owner = root.IsPackage ? root.AssetPrefix : OwnerInAssets(assetPath),
            };
            ElfInfo info; string error;
            if (ElfReader.TryReadFile(file, out info, out error)) lib.Elf = info; else lib.ReadError = error;

            lib.Abi = AbiFromPath(assetPath) ?? (info == null ? null : ElfReader.AbiFromMachine(info.Machine));
            return lib;
        }

        static void ScanAar(string file, string assetPath, List<NativeLib> libs)
        {
            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    foreach (var entry in zip.Entries)
                    {
                        string[] parts = entry.FullName.Split('/');
                        // jni/<abi>/<name>.so
                        if (parts.Length != 3 || parts[0] != "jni" || Array.IndexOf(Abis, parts[1]) < 0) continue;
                        if (!parts[2].EndsWith(".so", StringComparison.OrdinalIgnoreCase)) continue;

                        var lib = new NativeLib
                        {
                            Name = parts[2], Abi = parts[1], Owner = assetPath,
                            Location = assetPath + "!/" + entry.FullName,
                        };
                        string error;
                        lib.Elf = ZipElfReader.Read(entry, out error);
                        if (lib.Elf == null) lib.ReadError = error;
                        libs.Add(lib);
                    }
                }
            }
            catch (InvalidDataException e) { libs.Add(Unreadable(file, assetPath, e.Message)); }
            catch (IOException e) { libs.Add(Unreadable(file, assetPath, e.Message)); }
            catch (UnauthorizedAccessException e) { libs.Add(Unreadable(file, assetPath, e.Message)); }
        }

        static NativeLib Unreadable(string file, string assetPath, string reason)
        {
            return new NativeLib { Name = Path.GetFileName(file), Location = assetPath, Owner = assetPath, ReadError = reason };
        }

        /// <summary>The ABI named by a folder in the path ("arm64-v8a" in "Assets/Plugins/arm64-v8a/x.so"), or null.</summary>
        internal static string AbiFromPath(string assetPath)
        {
            foreach (string part in assetPath.Split('/'))
                if (Array.IndexOf(Abis, part) >= 0) return part;
            return null;
        }

        /// <summary>
        /// The owner of a loose .so under Assets: the first folder of its path that is not generic (Plugins,
        /// Android, libs, lib, jni) and comes before the ABI folder. If every folder is generic, the folder that
        /// contains the ABI folder. The owner is written as the asset path of that folder.
        /// </summary>
        internal static string OwnerInAssets(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            int folderCount = parts.Length - 1; // the last part is the file name
            int abiIndex = -1;
            for (int i = 0; i < folderCount; i++)
                if (Array.IndexOf(Abis, parts[i]) >= 0) { abiIndex = i; break; }

            int limit = abiIndex >= 0 ? abiIndex : folderCount; // folders considered: parts[1 .. limit-1]
            for (int i = 1; i < limit; i++)
                if (!GenericFolders.Contains(parts[i])) return string.Join("/", parts, 0, i + 1);

            // Every folder is generic: use the folder that holds the ABI folder (or the file).
            return string.Join("/", parts, 0, Math.Max(1, limit));
        }

        static string RelativeTo(string root, string file)
        {
            return ProjectFileWalker.ToProjectRelative(root, file);
        }
    }
}
