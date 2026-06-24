using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    public static class RecordLoggingService
    {
        private static RecordLoggingConfig _config;
        private static StreamWriter _writer;
        private static ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        private static StringBuilder _sb = new StringBuilder();
        private static readonly object _lock = new object();
        private static string _logDirectory;
        private static string _currentFilePath;
        private static bool _isInitialized;

        public static bool IsInitialized => _isInitialized;
        public static int QueueCount => _queue.Count;
        public static string CurrentFilePath => _currentFilePath;

        public static void Initialize(RecordLoggingConfig config)
        {
            if (_isInitialized) return;
            if (config == null) return;

            _config = config;
            _logDirectory = Path.Combine(Application.persistentDataPath, "RecordLogging");

            if (_config.snapshotMode)
            {
                string snapshotsDir = Path.Combine(_logDirectory, "Snapshots");
                Directory.CreateDirectory(snapshotsDir);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                _currentFilePath = Path.Combine(snapshotsDir, $"log-{timestamp}.txt");
            }
            else
            {
                Directory.CreateDirectory(_logDirectory);
                _currentFilePath = Path.Combine(_logDirectory, _config.fileName);
            }

            FileMode fileMode = _config.appendMode ? FileMode.Append : FileMode.Create;
            FileStream fs = new FileStream(_currentFilePath, fileMode, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(fs, Encoding.UTF8);

            _queue = new ConcurrentQueue<string>();
            _sb = new StringBuilder();

            Application.logMessageReceived += OnLogMessageReceived;
            Application.quitting += OnApplicationQuitting;

            _isInitialized = true;
            Debug.Log($"[RecordLogging] Initialized -> {_currentFilePath}");
        }

        public static void Shutdown()
        {
            if (!_isInitialized) return;

            Application.logMessageReceived -= OnLogMessageReceived;
            Application.quitting -= OnApplicationQuitting;

            FlushInternal();

            lock (_lock)
            {
                if (_writer != null)
                {
                    _writer.Flush();
                    _writer.Dispose();
                    _writer = null;
                }
            }

            _isInitialized = false;
        }

        private static void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (!_isInitialized) return;
            if (!_config.isEnabled) return;

            if (!LogTypeFilterMatches(type)) return;

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string entry = _config.detailLevel == LogDetailLevel.Detail
                ? $"[{timestamp}] [{type}] {logString}\n{stackTrace}"
                : $"[{timestamp}] [{type}] {logString}";
            _queue.Enqueue(entry);

            if (_queue.Count >= _config.queueSize)
            {
                FlushInternal();
            }
        }

        private static bool LogTypeFilterMatches(LogType type)
        {
            switch (type)
            {
                case LogType.Log: return (_config.logFilter & LogTypeFlags.Log) != 0;
                case LogType.Warning: return (_config.logFilter & LogTypeFlags.Warning) != 0;
                case LogType.Error: return (_config.logFilter & LogTypeFlags.Error) != 0;
                case LogType.Exception: return (_config.logFilter & LogTypeFlags.Exception) != 0;
                case LogType.Assert: return (_config.logFilter & LogTypeFlags.Assert) != 0;
                default: return false;
            }
        }

        public static void Flush()
        {
            FlushInternal();
        }

        private static void FlushInternal()
        {
            if (!_isInitialized) return;

            lock (_lock)
            {
                if (_writer == null) return;

                _sb.Clear();
                while (_queue.TryDequeue(out string entry))
                {
                    _sb.AppendLine(entry);
                }

                if (_sb.Length > 0)
                {
                    _writer.Write(_sb.ToString());
                    _writer.Flush();
                }
            }
        }

        private static void OnApplicationQuitting()
        {
            Shutdown();
        }

        public static void ClearLogs()
        {
            Shutdown();

            if (Directory.Exists(_logDirectory))
            {
                Directory.Delete(_logDirectory, true);
            }

            _currentFilePath = null;
        }

        public static string GetLogDirectory()
        {
            return _logDirectory;
        }
    }
}
