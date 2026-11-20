using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    public static class SampleImporter
    {
        private const string SamplesFolder = "Assets/Samples";

        public static string FindPackageRoot(string editorAssemblyName)
        {
            foreach (var guid in AssetDatabase.FindAssets(editorAssemblyName + " t:AssemblyDefinitionAsset"))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(assetPath) != editorAssemblyName)
                {
                    continue;
                }

                var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(assetPath);
                if (package != null)
                {
                    return package.resolvedPath;
                }

                return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(assetPath)));
            }

            return null;
        }

        public static int ImportAll(string packageRoot, string packageName)
        {
            if (packageRoot == null)
            {
                Debug.LogError($"The {packageName} package folder was not found.");
                return 0;
            }

            var package = ReadPackage(File.ReadAllText(Path.Combine(packageRoot, "package.json")));
            var imported = 0;
            try
            {
                foreach (var sample in package.samples)
                {
                    var existing = FindExistingImport(package.displayName, sample.displayName);
                    if (existing != null)
                    {
                        Debug.LogWarning($"'{sample.displayName}' is already in {existing}. Delete that folder to import it again.");
                        continue;
                    }

                    Copy(Path.Combine(packageRoot, sample.path), DestinationFor(package.displayName, sample.displayName));
                    imported++;
                }
            }
            finally
            {
                AssetDatabase.Refresh();
            }

            if (imported == 0)
            {
                Debug.LogWarning($"Nothing was imported from {package.displayName}.");
            }
            else
            {
                Debug.Log($"Imported {imported} sample(s) from {package.displayName} into {SamplesFolder}.");
            }

            return imported;
        }

        public static PackageManifest ReadPackage(string packageJson)
        {
            var package = string.IsNullOrWhiteSpace(packageJson) ? null : JsonUtility.FromJson<PackageManifest>(packageJson);
            package = package ?? new PackageManifest();
            package.samples = package.samples ?? new SampleEntry[0];
            return package;
        }

        public static string DestinationFor(string packageDisplayName, string sampleDisplayName)
        {
            return $"{SamplesFolder}/{packageDisplayName}/{sampleDisplayName}";
        }

        public static string FindExistingImport(string packageDisplayName, string sampleDisplayName)
        {
            var direct = DestinationFor(packageDisplayName, sampleDisplayName);
            if (Directory.Exists(direct))
            {
                return direct;
            }

            var packageFolder = $"{SamplesFolder}/{packageDisplayName}";
            if (!Directory.Exists(packageFolder))
            {
                return null;
            }

            foreach (var versionFolder in Directory.GetDirectories(packageFolder))
            {
                var versioned = Path.Combine(versionFolder, sampleDisplayName).Replace('\\', '/');
                if (Directory.Exists(versioned))
                {
                    return versioned;
                }
            }

            return null;
        }

        public static void Copy(string source, string destination)
        {
            if (!Directory.Exists(source))
            {
                throw new DirectoryNotFoundException($"The sample folder {source} does not exist.");
            }

            if (Directory.Exists(destination))
            {
                throw new IOException($"{destination} already exists.");
            }

            var partial = destination + ".importing";
            if (Directory.Exists(partial))
            {
                Directory.Delete(partial, true);
            }

            try
            {
                CopyContents(source, partial);
                Directory.Move(partial, destination);
            }
            catch
            {
                if (Directory.Exists(partial))
                {
                    Directory.Delete(partial, true);
                }

                throw;
            }
        }

        private static void CopyContents(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
            }

            foreach (var folder in Directory.GetDirectories(source))
            {
                CopyContents(folder, Path.Combine(destination, Path.GetFileName(folder)));
            }
        }

        [Serializable]
        public class PackageManifest
        {
            public string name;
            public string displayName;
            public string version;
            public string requiresCore;
            public SampleEntry[] samples;
        }

        [Serializable]
        public class SampleEntry
        {
            public string displayName;
            public string description;
            public string path;
        }
    }
}
