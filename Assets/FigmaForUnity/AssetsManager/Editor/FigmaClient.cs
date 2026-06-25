using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FigmaForUnity.Editor
{
    public class FigmaClient
    {
        private readonly string _token;
        private const string BaseUrl = "https://api.figma.com/v1";

        public FigmaClient(string token)
        {
            _token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public static string ParseFileKey(string figmaUrl)
        {
            if (string.IsNullOrWhiteSpace(figmaUrl))
                return null;

            var pattern = @"figma\.com/(file|proto|design)/([a-zA-Z0-9]+)";
            var match = Regex.Match(figmaUrl, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[2].Value : null;
        }

        public async Task<JObject> GetFileAsync(string fileKey, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileKey))
                throw new ArgumentNullException(nameof(fileKey));

            var url = $"{BaseUrl}/files/{fileKey}";
            var json = await GetAsync(url, ct: ct);
            return JObject.Parse(json);
        }

        public async Task<Dictionary<string, string>> GetImageUrlsAsync(
            string fileKey, List<string> nodeIds, string format = "png", float scale = 1f,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileKey))
                throw new ArgumentNullException(nameof(fileKey));
            if (nodeIds == null || nodeIds.Count == 0)
                throw new ArgumentException("Node IDs must not be empty.", nameof(nodeIds));

            var ids = string.Join(",", nodeIds.Select(Uri.EscapeDataString));
            var url = $"{BaseUrl}/images/{fileKey}?ids={ids}&format={Uri.EscapeDataString(format)}&scale={scale}";
            var json = await GetAsync(url, ct: ct);
            var result = JObject.Parse(json);
            var images = result["images"] as JObject;
            if (images == null)
                return new Dictionary<string, string>();

            return images.Properties()
                .ToDictionary(p => p.Name, p => p.Value.ToString());
        }

        private async Task<string> GetAsync(string url, int maxAttempts = 3, CancellationToken ct = default)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                using var request = UnityWebRequest.Get(url);
                request.SetRequestHeader("X-FIGMA-TOKEN", _token);

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                    return request.downloadHandler.text;

                if (request.responseCode == 429 && attempt < maxAttempts)
                {
                    var wait = Mathf.Pow(2, attempt - 1) * 1000;
                    Debug.LogWarning($"[FigmaSync] Rate limited. Retrying in {wait}ms...");
                    await Task.Delay((int)wait, ct);
                    continue;
                }

                if (request.responseCode == 403)
                    throw new UnauthorizedAccessException(
                        "Figma token invalid or expired. Please check your token.");

                throw new Exception($"Figma API error {request.responseCode}: {request.error}");
            }

            throw new Exception("Figma API request failed after retries.");
        }
    }
}
