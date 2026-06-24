using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    public class RecordLoggingWindow : EditorWindow
    {
        private RecordLoggingConfig _config;
        private Vector2 _scrollPos;
        private int _lastLogCount;

        [MenuItem("Tools/TnieYu/Record Logging")]
        public static void Open()
        {
            var window = GetWindow<RecordLoggingWindow>("Record Logging");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            _config = RecordLoggingConfig.Instance;
            if (_config == null)
            {
                string[] guids = AssetDatabase.FindAssets("RecordLoggingConfig t:ScriptableObject");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _config = AssetDatabase.LoadAssetAtPath<RecordLoggingConfig>(path);
                }
            }

            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<RecordLoggingConfig>();
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateAsset(_config, "Assets/Resources/RecordLoggingConfig.asset");
                AssetDatabase.SaveAssets();
            }

            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (RecordLoggingService.IsInitialized)
            {
                int currentCount = RecordLoggingService.QueueCount;
                if (currentCount != _lastLogCount)
                {
                    _lastLogCount = currentCount;
                    Repaint();
                }
            }
        }

        private void OnGUI()
        {
            if (_config == null)
            {
                EditorGUILayout.HelpBox("Config not found. Please create a RecordLoggingConfig.", MessageType.Warning);
                return;
            }

            bool isPlaying = EditorApplication.isPlaying;
            bool serviceActive = RecordLoggingService.IsInitialized;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("Record Logging Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledGroupScope(!isPlaying))
            {
                _config.isEnabled = EditorGUILayout.Toggle("Enable Logging", _config.isEnabled);
            }

            if (!isPlaying)
            {
                EditorGUILayout.HelpBox("Logging only works during Play Mode.", MessageType.Info);
            }

            EditorGUILayout.Space();
            GUILayout.Label("File Settings", EditorStyles.boldLabel);

            _config.fileName = EditorGUILayout.TextField("File Name", _config.fileName);
            _config.queueSize = EditorGUILayout.IntField("Queue Size", _config.queueSize);
            if (_config.queueSize < 1) _config.queueSize = 1;

            using (new EditorGUI.DisabledGroupScope(_config.snapshotMode))
            {
                _config.appendMode = EditorGUILayout.Toggle("Append Mode", _config.appendMode);
            }

            _config.snapshotMode = EditorGUILayout.Toggle("Snapshot Mode", _config.snapshotMode);

            EditorGUILayout.Space();
            GUILayout.Label("Log Content", EditorStyles.boldLabel);
            _config.detailLevel = (LogDetailLevel)EditorGUILayout.EnumPopup("Detail Level", _config.detailLevel);

            EditorGUILayout.Space();
            GUILayout.Label("Log Filter", EditorStyles.boldLabel);
            _config.logFilter = (LogTypeFlags)EditorGUILayout.EnumFlagsField("Log Types", _config.logFilter);

            EditorGUILayout.Space();
            GUILayout.Label("Actions", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledGroupScope(!isPlaying))
            {
                if (GUILayout.Button("Flush Now", GUILayout.Height(30)))
                {
                    RecordLoggingService.Flush();
                }
            }

            if (GUILayout.Button("Clear All Logs", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Logs",
                    "Are you sure you want to delete all log files in the RecordLogging directory?",
                    "Yes", "Cancel"))
                {
                    RecordLoggingService.ClearLogs();
                    _lastLogCount = 0;
                }
            }

            if (GUILayout.Button("Open Log Folder", GUILayout.Height(30)))
            {
                string dir = RecordLoggingService.GetLogDirectory();
                if (dir != null && System.IO.Directory.Exists(dir))
                    EditorUtility.RevealInFinder(dir);
                else
                    EditorUtility.RevealInFinder(Application.persistentDataPath);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Status", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Service Active", serviceActive ? "Yes" : "No");
            EditorGUILayout.LabelField("Queue", $"{_lastLogCount} / {_config.queueSize}");

            if (serviceActive)
            {
                EditorGUILayout.LabelField("Log File", RecordLoggingService.CurrentFilePath);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_config);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
