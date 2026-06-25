using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace FigmaForUnity.Editor
{
    public class FigmaSyncWindow : EditorWindow
    {
        private const string PrefKeyUrl = "FigmaSync_Url";
        private const string PrefKeyToken = "FigmaSync_Token";
        private const string PrefKeyFolder = "FigmaSync_Folder";

        [MenuItem("Tools/Figma Asset Sync")]
        public static void ShowWindow()
        {
            GetWindow<FigmaSyncWindow>("Figma Asset Sync");
        }

        private string _figmaUrl;
        private string _token;
        private string _localFolder;
        private string _statusMessage = "● Ready";
        private Color _statusColor = Color.green;
        private FigmaManifest _manifest;
        private ChangeReport _lastReport;
        private bool _isBusy;
        private CancellationTokenSource _cts;
        private List<string> _pageNames;
        private HashSet<string> _selectedPages;
        private int _activeTab;
        private Vector2 _assetListScrollPos;
        private Vector2 _manifestScrollPos;

        private void OnEnable()
        {
            _figmaUrl = EditorPrefs.GetString(PrefKeyUrl, "");
            _token = EditorPrefs.GetString(PrefKeyToken, "");
            _localFolder = EditorPrefs.GetString(PrefKeyFolder, "Assets/UI/Sprites");
            _manifest = ManifestManager.Load(_localFolder);
            if (_manifest != null)
            {
                var validation = ManifestManager.Validate(_manifest, _localFolder);
                if (validation.HasIssues)
                {
                    var msgs = new List<string>();
                    if (validation.RemovedCount > 0)
                    {
                        var names = string.Join(", ", validation.RemovedNames);
                        msgs.Add($"removed {validation.RemovedCount} missing entries");
                        Debug.LogWarning($"[FigmaSync] Manifest validated: removed {validation.RemovedCount} missing entries: {names}");
                    }
                    if (validation.NewFileCount > 0)
                        msgs.Add($"found {validation.NewFileCount} unregistered files");
                    _statusMessage = $"● Warning: {string.Join(", ", msgs)}";
                    _statusColor = Color.yellow;
                }
            }
        }

        private void OnDisable()
        {
            SavePrefs();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawInputs();
            DrawStatus();
            var tabs = new[] { "Sync", "Manifest" };
            _activeTab = GUILayout.Toolbar(_activeTab, tabs);
            EditorGUILayout.Space();
            switch (_activeTab)
            {
                case 0:
                    DrawSyncPanel();
                    break;
                case 1:
                    DrawManifestPanel();
                    break;
            }
        }

        private void DrawHeader()
        {
            GUILayout.Label("Figma Asset Sync", EditorStyles.largeLabel);
            EditorGUILayout.Space();
        }

        private void DrawInputs()
        {
            _figmaUrl = EditorGUILayout.TextField("Figma URL", _figmaUrl);
            _token = EditorGUILayout.PasswordField("Token", _token);

            EditorGUILayout.BeginHorizontal();
            _localFolder = EditorGUILayout.TextField("Local Folder", _localFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var selected = EditorUtility.OpenFolderPanel("Select Folder", _localFolder, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    var dataPath = Application.dataPath;
                    _localFolder = selected.StartsWith(dataPath)
                        ? "Assets" + selected.Substring(dataPath.Length)
                        : selected;
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatus()
        {
            EditorGUILayout.Space();
            var prevColor = GUI.color;
            GUI.color = _statusColor;
            GUILayout.Label(_statusMessage);
            GUI.color = prevColor;
            EditorGUILayout.Space();
        }

        private void DrawSyncToolbar()
        {
            EditorGUI.BeginDisabledGroup(_isBusy);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Install All", GUILayout.Height(22)))
                _ = RunInstallAll();
            if (GUILayout.Button("Check Changes", GUILayout.Height(22)))
                _ = RunCheckChanges();
            if (_lastReport != null && _lastReport.HasChanges)
            {
                var count = _lastReport.ToInstall.Count + _lastReport.ToUpdate.Count;
                if (GUILayout.Button($"Refresh ({count})", GUILayout.Height(22)))
                    _ = RunRefreshChanged();
            }
            if (GUILayout.Button("↻ Load Pages", GUILayout.Height(22)))
                _ = RunLoadPages();
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

            if (_isBusy)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Cancel", GUILayout.Height(22)))
                    _cts?.Cancel();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPageFilter()
        {
            if (_pageNames == null || _pageNames.Count == 0)
                return;

            if (_pageNames.Count == 1)
            {
                EditorGUILayout.LabelField($"Page: {_pageNames[0]}", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.LabelField("Filter by Page", EditorStyles.boldLabel);

            var stats = GetPageStats();
            foreach (var page in _pageNames)
            {
                EditorGUILayout.BeginHorizontal();
                bool wasSelected = _selectedPages.Contains(page);
                bool isSelected = EditorGUILayout.Toggle(wasSelected, GUILayout.Width(16));
                if (isSelected != wasSelected)
                {
                    if (isSelected) _selectedPages.Add(page);
                    else _selectedPages.Remove(page);
                    Repaint();
                }
                EditorGUILayout.LabelField(page, GUILayout.Width(100));
                if (_manifest != null)
                {
                    var count = _manifest.Assets.Values.Count(a => (a.PageName ?? "Unknown") == page);
                    if (count > 0)
                        EditorGUILayout.LabelField($"({count})", GUILayout.Width(40));
                    else
                        EditorGUILayout.LabelField("(0)", GUILayout.Width(40));
                }
                if (stats.TryGetValue(page, out var s))
                    EditorGUILayout.LabelField($"({s.New}n, {s.Updated}u, {s.Unchanged}\u2713)");
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawAssetListPanel()
        {
            _assetListScrollPos = EditorGUILayout.BeginScrollView(_assetListScrollPos, GUILayout.ExpandHeight(true));

            if (_manifest != null && _manifest.Assets.Count > 0)
            {
                var grouped = _manifest.Assets
                    .GroupBy(kvp => kvp.Value.PageName ?? "Unknown")
                    .OrderBy(g => g.Key);

                foreach (var group in grouped)
                {
                    EditorGUILayout.LabelField($"  {group.Key}", EditorStyles.boldLabel);
                    foreach (var kvp in group)
                    {
                        var asset = kvp.Value;
                        var icon = GetStatusIcon(kvp.Key);
                        EditorGUILayout.LabelField($"  {icon} {asset.Name}");
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSyncPanel()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(180));
            DrawPageFilter();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawSyncToolbar();
            DrawAssetListPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawManifestPanel()
        {
            if (_manifest == null)
            {
                EditorGUILayout.HelpBox("No manifest found. Run Install All first.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Manifest Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("File Key:", _manifest.FigmaFileKey);
            EditorGUILayout.LabelField("Last Sync:", _manifest.LastSync?.ToString("yyyy-MM-dd HH:mm:ss UTC"));
            EditorGUILayout.LabelField("Total Assets:", _manifest.Assets.Count.ToString());
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Manifest", GUILayout.Height(22)))
            {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                var path = Path.Combine(projectRoot, _localFolder, "figma_manifest.json");
                EditorUtility.OpenWithDefaultApp(path);
            }
            if (GUILayout.Button("Validate", GUILayout.Height(22)))
            {
                var result = ManifestManager.Validate(_manifest, _localFolder);
                var messages = new List<string>();
                if (result.RemovedCount > 0)
                    messages.Add($"cleaned {result.RemovedCount} missing entries");
                if (result.NewFileCount > 0)
                    messages.Add($"found {result.NewFileCount} unregistered files");
                if (messages.Count > 0)
                    UpdateStatus($"\u25cf Manifest: {string.Join(", ", messages)}", Color.yellow);
                else
                    UpdateStatus("\u25cf Manifest is clean", Color.green);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Entries", EditorStyles.boldLabel);
            _manifestScrollPos = EditorGUILayout.BeginScrollView(_manifestScrollPos, GUILayout.ExpandHeight(true));
            foreach (var kvp in _manifest.Assets.OrderBy(e => e.Value.PageName).ThenBy(e => e.Value.Name))
            {
                EditorGUILayout.LabelField($"  [{kvp.Value.PageName}] {kvp.Value.Name}");
            }
            EditorGUILayout.EndScrollView();
        }

        private string GetStatusIcon(string nodeId)
        {
            if (_lastReport == null) return "✓";
            if (_lastReport.ToInstall.Any(n => n.NodeId == nodeId)) return "🆕";
            if (_lastReport.ToUpdate.Any(n => n.NodeId == nodeId)) return "🔄";
            return "✅";
        }

        private Dictionary<string, (int New, int Updated, int Unchanged)> GetPageStats()
        {
            var result = new Dictionary<string, (int New, int Updated, int Unchanged)>();
            if (_lastReport == null) return result;

            var allPageNames = _pageNames ?? new List<string> { "Unknown" };
            foreach (var page in allPageNames)
            {
                var p = page;
                result[p] = (
                    _lastReport.ToInstall.Count(n => (n.PageName ?? "Unknown") == p),
                    _lastReport.ToUpdate.Count(n => (n.PageName ?? "Unknown") == p),
                    _lastReport.Unchanged.Count(n => (n.PageName ?? "Unknown") == p)
                );
            }

            return result;
        }

        private void SavePrefs()
        {
            EditorPrefs.SetString(PrefKeyUrl, _figmaUrl);
            EditorPrefs.SetString(PrefKeyToken, _token);
            EditorPrefs.SetString(PrefKeyFolder, _localFolder);
        }

        private void UpdateStatus(string message, Color color)
        {
            _statusMessage = message;
            _statusColor = color;
            Repaint();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(_figmaUrl))
            {
                UpdateStatus("● Please enter a Figma URL.", Color.red);
                return false;
            }
            if (FigmaClient.ParseFileKey(_figmaUrl) == null)
            {
                UpdateStatus("● Invalid Figma URL format.", Color.red);
                return false;
            }
            if (string.IsNullOrWhiteSpace(_token))
            {
                UpdateStatus("● Please enter your Figma token.", Color.red);
                return false;
            }
            if (string.IsNullOrWhiteSpace(_localFolder))
            {
                UpdateStatus("● Please enter a local folder path.", Color.red);
                return false;
            }
            if (!_localFolder.StartsWith("Assets/") && !_localFolder.StartsWith("Assets\\"))
            {
                UpdateStatus("● Local folder must be under Assets/.", Color.red);
                return false;
            }
            return true;
        }

        private CancellationToken GetToken()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }

        private async Task RunLoadPages()
        {
            if (string.IsNullOrWhiteSpace(_figmaUrl))
            {
                UpdateStatus("● Please enter a Figma URL.", Color.red);
                return;
            }
            if (FigmaClient.ParseFileKey(_figmaUrl) == null)
            {
                UpdateStatus("● Invalid Figma URL format.", Color.red);
                return;
            }
            if (string.IsNullOrWhiteSpace(_token))
            {
                UpdateStatus("● Please enter your Figma token.", Color.red);
                return;
            }

            _isBusy = true;
            SavePrefs();
            var ct = GetToken();

            try
            {
                UpdateStatus("○ Fetching document tree...", Color.yellow);
                var client = new FigmaClient(_token);
                var fileKey = FigmaClient.ParseFileKey(_figmaUrl);
                var document = await client.GetFileAsync(fileKey, ct);

                UpdateStatus("○ Parsing pages...", Color.yellow);
                var nodes = AssetMapper.ExtractImageNodes(document);
                PopulatePages(nodes);

                var pageCount = _pageNames.Count;
                UpdateStatus($"● Loaded {pageCount} page(s): {string.Join(", ", _pageNames)}", Color.green);
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("● Cancelled.", Color.yellow);
            }
            catch (Exception ex)
            {
                UpdateStatus($"● Error: {ex.Message}", Color.red);
                Debug.LogError($"[FigmaSync] {ex}");
            }
            finally
            {
                _isBusy = false;
                Repaint();
            }
        }

        private async Task RunInstallAll()
        {
            if (!ValidateInputs()) return;
            _isBusy = true;
            SavePrefs();

            var ct = GetToken();
            try
            {
                UpdateStatus("○ Fetching document tree...", Color.yellow);
                var client = new FigmaClient(_token);
                var fileKey = FigmaClient.ParseFileKey(_figmaUrl);
                var document = await client.GetFileAsync(fileKey, ct);

                UpdateStatus("○ Parsing image nodes...", Color.yellow);
                var nodes = AssetMapper.ExtractImageNodes(document);
                PopulatePages(nodes);

                if (nodes.Count == 0)
                {
                    UpdateStatus("● No image nodes found in document.", Color.yellow);
                    return;
                }

                nodes = FilterBySelectedPages(nodes);
                var nodeIds = nodes.Select(n => n.NodeId).ToList();
                var nodeMap = nodes.ToDictionary(n => n.NodeId);

                UpdateStatus("○ Fetching download URLs...", Color.yellow);
                var urls = await client.GetImageUrlsAsync(fileKey, nodeIds, ct: ct);

                UpdateStatus("○ Downloading assets...", Color.yellow);
                var progress = new Progress<float>(p =>
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Downloading Assets", $"Progress: {p:P0}", p))
                    {
                        _cts?.Cancel();
                    }
                });

                var succeeded = await ImageDownloader.DownloadBatchAsync(urls, nodeMap, _localFolder, progress, ct);
                var installedNodes = nodes.Where(n => succeeded.Contains(n.NodeId)).ToList();

                if (_manifest == null)
                {
                    _manifest = new FigmaManifest
                    {
                        FigmaFileKey = fileKey,
                        LocalFolderPath = _localFolder,
                        LastSync = DateTime.UtcNow,
                        Assets = new Dictionary<string, ManifestAsset>()
                    };
                }
                else
                {
                    _manifest.FigmaFileKey = fileKey;
                    _manifest.LocalFolderPath = _localFolder;
                    _manifest.LastSync = DateTime.UtcNow;
                    if (_manifest.Assets == null)
                        _manifest.Assets = new Dictionary<string, ManifestAsset>();
                }

                foreach (var node in installedNodes)
                {
                    _manifest.Assets[node.NodeId] = new ManifestAsset
                    {
                        Name = node.Name,
                        ImageRef = node.ImageRef,
                        PageName = node.PageName,
                        LocalPath = Path.Combine(_localFolder, SanitizeFileName(node.PageName), $"{SanitizeFileName(node.Name)}.png")
                    };
                }

                ManifestManager.Save(_manifest, _localFolder);
                _lastReport = null;

                var count = installedNodes.Count;
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh();
                    UpdateStatus($"● Install complete — {count} assets", Color.green);
                    _isBusy = false;
                    EditorUtility.ClearProgressBar();
                    Repaint();
                };
                return;
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("● Cancelled.", Color.yellow);
            }
            catch (UnauthorizedAccessException ex)
            {
                UpdateStatus($"● {ex.Message}", Color.red);
                EditorPrefs.DeleteKey(PrefKeyToken);
            }
            catch (Exception ex)
            {
                UpdateStatus($"● Error: {ex.Message}", Color.red);
                Debug.LogError($"[FigmaSync] {ex}");
            }

            EditorUtility.ClearProgressBar();
            _isBusy = false;
            Repaint();
        }

        private async Task RunCheckChanges()
        {
            if (!ValidateInputs()) return;
            _isBusy = true;
            SavePrefs();

            var ct = GetToken();
            try
            {
                UpdateStatus("○ Fetching document tree...", Color.yellow);
                var client = new FigmaClient(_token);
                var fileKey = FigmaClient.ParseFileKey(_figmaUrl);
                var document = await client.GetFileAsync(fileKey, ct);

                UpdateStatus("○ Comparing with manifest...", Color.yellow);
                var nodes = AssetMapper.ExtractImageNodes(document);
                PopulatePages(nodes);

                _manifest = ManifestManager.Load(_localFolder);
                if (_manifest == null)
                {
                    UpdateStatus("● No manifest found. Run Install All first.", Color.yellow);
                    return;
                }

                nodes = FilterBySelectedPages(nodes);
                _lastReport = ChangeDetector.Compare(nodes, _manifest);
                FilterReportBySelectedPages();

                if (_lastReport.HasChanges)
                {
                    UpdateStatus(
                        $"● Changes found: {_lastReport.ToInstall.Count} new, {_lastReport.ToUpdate.Count} updated",
                        Color.yellow);
                }
                else
                {
                    UpdateStatus("● No changes detected — all assets up to date.", Color.green);
                }
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("● Cancelled.", Color.yellow);
            }
            catch (Exception ex)
            {
                UpdateStatus($"● Error: {ex.Message}", Color.red);
                Debug.LogError($"[FigmaSync] {ex}");
            }
            finally
            {
                _isBusy = false;
                Repaint();
            }
        }

        private async Task RunRefreshChanged()
        {
            if (_lastReport == null || !_lastReport.HasChanges) return;
            if (!ValidateInputs()) return;
            _isBusy = true;
            SavePrefs();

            var ct = GetToken();
            try
            {
                var client = new FigmaClient(_token);
                var fileKey = FigmaClient.ParseFileKey(_figmaUrl);

                var changedNodes = _lastReport.ToInstall.Concat(_lastReport.ToUpdate).ToList();
                var nodeIds = changedNodes.Select(n => n.NodeId).ToList();
                var nodeMap = changedNodes.ToDictionary(n => n.NodeId);

                UpdateStatus("○ Fetching download URLs...", Color.yellow);
                var urls = await client.GetImageUrlsAsync(fileKey, nodeIds, ct: ct);

                UpdateStatus("○ Downloading changed assets...", Color.yellow);
                var progress = new Progress<float>(p =>
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Downloading Assets", $"Progress: {p:P0}", p))
                    {
                        _cts?.Cancel();
                    }
                });

                var succeeded = await ImageDownloader.DownloadBatchAsync(urls, nodeMap, _localFolder, progress, ct);

                foreach (var node in changedNodes.Where(n => succeeded.Contains(n.NodeId)))
                {
                    _manifest.Assets[node.NodeId] = new ManifestAsset
                    {
                        Name = node.Name,
                        ImageRef = node.ImageRef,
                        PageName = node.PageName,
                        LocalPath = Path.Combine(_localFolder, SanitizeFileName(node.PageName), $"{SanitizeFileName(node.Name)}.png")
                    };
                }
                _manifest.LastSync = DateTime.UtcNow;
                ManifestManager.Save(_manifest, _localFolder);

                var updatedCount = changedNodes.Count(n => succeeded.Contains(n.NodeId));
                _lastReport = null;

                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh();
                    UpdateStatus($"● Refresh complete — {updatedCount} updated", Color.green);
                    _isBusy = false;
                    EditorUtility.ClearProgressBar();
                    Repaint();
                };
                return;
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("● Cancelled.", Color.yellow);
            }
            catch (Exception ex)
            {
                UpdateStatus($"● Error: {ex.Message}", Color.red);
                Debug.LogError($"[FigmaSync] {ex}");
            }

            EditorUtility.ClearProgressBar();
            _isBusy = false;
            Repaint();
        }

        private void PopulatePages(List<FigmaImageNode> nodes)
        {
            var oldPageNames = _pageNames ?? new List<string>();
            _pageNames = nodes
                .Select(n => n.PageName ?? "Unknown")
                .Distinct()
                .OrderBy(n => n)
                .ToList();
            if (_selectedPages == null)
            {
                _selectedPages = new HashSet<string>(_pageNames);
            }
            else
            {
                foreach (var p in _pageNames)
                {
                    if (!oldPageNames.Contains(p))
                        _selectedPages.Add(p);
                }
            }
        }

        private List<FigmaImageNode> FilterBySelectedPages(List<FigmaImageNode> nodes)
        {
            if (_selectedPages == null || _selectedPages.Count == 0)
                return nodes;
            return nodes.Where(n => _selectedPages.Contains(n.PageName ?? "Unknown")).ToList();
        }

        private void FilterReportBySelectedPages()
        {
            if (_lastReport == null || _selectedPages == null || _selectedPages.Count == 0)
                return;
            _lastReport.ToInstall = _lastReport.ToInstall
                .Where(n => _selectedPages.Contains(n.PageName ?? "Unknown"))
                .ToList();
            _lastReport.ToUpdate = _lastReport.ToUpdate
                .Where(n => _selectedPages.Contains(n.PageName ?? "Unknown"))
                .ToList();
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "asset";
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }
    }
}
