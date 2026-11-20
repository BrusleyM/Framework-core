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
                if (Path.GetFileNameWithoutExtension(assetPath) == editorAssemblyName)
                {
                    var editorFolder = Path.GetDirectoryName(Path.GetFullPath(assetPath));
                    return Path.GetDirectoryName(editorFolder);
                }
            }

            return null;
        }

        public static void ImportAll(string packageRoot, string menuName)
        {
            if (packageRoot == null)
            {
                Debug.LogError($"The {menuName} package folder was not found.");
                return;
            }

            var package = ReadPackage(File.ReadAllText(Path.Combine(packageRoot, "package.json")));
            var imported = 0;
            foreach (var sample in package.samples)
            {
                var destination = DestinationFor(package.displayName, sample.displayName);
                if (Directory.Exists(destination))
                {
                    Debug.LogWarning($"'{sample.displayName}' is already in {destination}. Delete that folder to import it again.");
                    continue;
                }

                Copy(Path.Combine(packageRoot, sample.path), destination);
                imported++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"Imported {imported} sample(s) from {package.displayName} into {SamplesFolder}.");
        }

        public static PackageSamples ReadPackage(string packageJson)
        {
            var package = JsonUtility.FromJson<PackageSamples>(packageJson);
            package.samples = package.samples ?? new SampleEntry[0];
            return package;
        }

        public static string DestinationFor(string packageDisplayName, string sampleDisplayName)
        {
            return $"{SamplesFolder}/{packageDisplayName}/{sampleDisplayName}";
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

            CopyContents(source, destination);
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
        public class PackageSamples
        {
            public string displayName;
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
