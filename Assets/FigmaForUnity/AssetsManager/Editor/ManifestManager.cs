using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace FigmaForUnity.Editor
{
    public static class ManifestManager
    {
        private static string GetManifestPath(string localFolder)
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(projectRoot, localFolder, "figma_manifest.json");
        }

        public static bool Exists(string localFolder) => File.Exists(GetManifestPath(localFolder));

        public static FigmaManifest Load(string localFolder)
        {
            try
            {
                if (!Exists(localFolder))
                    return null;

                string json = File.ReadAllText(GetManifestPath(localFolder));
                return JsonConvert.DeserializeObject<FigmaManifest>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FigmaSync] Manifest corrupt, will re-install: {ex.Message}");
                return null;
            }
        }

        public static void Save(FigmaManifest manifest, string localFolder)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
            string path = GetManifestPath(localFolder);
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, json);
            Debug.Log($"[FigmaSync] Manifest saved: {path}");
        }

        public static void Delete(string localFolder)
        {
            if (Exists(localFolder))
            {
                File.Delete(GetManifestPath(localFolder));
                Debug.Log($"[FigmaSync] Manifest deleted.");
            }
        }

        public static ValidationResult Validate(FigmaManifest manifest, string localFolder)
        {
            var result = new ValidationResult();
            if (manifest?.Assets == null || manifest.Assets.Count == 0)
                return result;

            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var toRemove = new List<string>();

            foreach (var kvp in manifest.Assets)
            {
                var asset = kvp.Value;
                if (string.IsNullOrEmpty(asset.LocalPath))
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }

                var absolutePath = Path.Combine(projectRoot, asset.LocalPath);
                if (!File.Exists(absolutePath))
                {
                    toRemove.Add(kvp.Key);
                    result.RemovedNodeIds.Add(kvp.Key);
                    result.RemovedNames.Add(asset.Name ?? "Unnamed");
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (var id in toRemove)
                    manifest.Assets.Remove(id);

                result.RemovedCount = toRemove.Count;
                Save(manifest, localFolder);
            }

            var manifestDir = Path.GetDirectoryName(GetManifestPath(localFolder));
            if (Directory.Exists(manifestDir))
            {
                var allPngFiles = Directory.GetFiles(manifestDir, "*.png", SearchOption.AllDirectories);
                var manifestPaths = new HashSet<string>(
                    manifest.Assets.Values
                        .Where(a => !string.IsNullOrEmpty(a.LocalPath))
                        .Select(a => Path.Combine(projectRoot, a.LocalPath).Replace('\\', '/'))
                );

                foreach (var file in allPngFiles)
                {
                    var normalized = file.Replace('\\', '/');
                    if (!manifestPaths.Contains(normalized))
                    {
                        result.NewFilePaths.Add(file);
                    }
                }
                result.NewFileCount = result.NewFilePaths.Count;
            }

            return result;
        }
    }

    public class ValidationResult
    {
        public int RemovedCount { get; set; }
        public List<string> RemovedNodeIds { get; set; } = new();
        public List<string> RemovedNames { get; set; } = new();
        public bool HasIssues => RemovedCount > 0 || NewFileCount > 0;

        public int NewFileCount { get; set; }
        public List<string> NewFilePaths { get; set; } = new();
    }
}
