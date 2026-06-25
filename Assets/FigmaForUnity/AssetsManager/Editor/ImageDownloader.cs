using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace FigmaForUnity.Editor
{
    public static class ImageDownloader
    {
        private static readonly HttpClient _httpClient;

        static ImageDownloader()
        {
            ServicePointManager.DefaultConnectionLimit = 10;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public static async Task<List<string>> DownloadBatchAsync(
            Dictionary<string, string> urlMap,
            Dictionary<string, FigmaImageNode> nodeMap,
            string localFolderPath,
            IProgress<float> progress = null,
            CancellationToken ct = default)
        {
            var succeeded = new List<string>();

            if (string.IsNullOrWhiteSpace(localFolderPath))
                throw new ArgumentNullException(nameof(localFolderPath));
            if (urlMap == null || urlMap.Count == 0)
                return succeeded;

            var fullPath = Path.GetFullPath(localFolderPath);
            Directory.CreateDirectory(fullPath);

            int total = urlMap.Count;
            int completed = 0;
            var lockObj = new object();
            using var semaphore = new SemaphoreSlim(4);

            var tasks = urlMap.Select(async kvp =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    ct.ThrowIfCancellationRequested();

                    if (!nodeMap.TryGetValue(kvp.Key, out var node))
                        return;

                    var fileName = SanitizeFileName(node.Name);
                    if (string.IsNullOrEmpty(fileName))
                        fileName = $"node_{kvp.Key.Replace(":", "_")}";

                    var pageFolder = SanitizeFileName(node.PageName ?? "Unknown");
                    var subPath = string.IsNullOrEmpty(pageFolder) ? fullPath : Path.Combine(fullPath, pageFolder);
                    Directory.CreateDirectory(subPath);
                    var filePath = Path.Combine(subPath, $"{fileName}.png");

                    try
                    {
                        var response = await _httpClient.GetAsync(kvp.Value, ct);
                        response.EnsureSuccessStatusCode();
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(filePath, bytes);
                        Debug.Log($"[FigmaSync] Saved: {filePath}");
                        lock (lockObj) succeeded.Add(kvp.Key);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[FigmaSync] Failed to download {node.Name}: {ex.Message}");
                    }
                }
                finally
                {
                    semaphore.Release();
                    Interlocked.Increment(ref completed);
                    progress?.Report((float)completed / total);
                }
            });

            await Task.WhenAll(tasks);
            return succeeded;
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            return sanitized.Trim();
        }
    }
}
