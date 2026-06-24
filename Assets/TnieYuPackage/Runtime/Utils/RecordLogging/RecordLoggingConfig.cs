using UnityEngine;

namespace TnieYuPackage.Utils
{
    [CreateAssetMenu(fileName = "RecordLoggingConfig", menuName = "TnieYu/Record Logging Config")]
    public class RecordLoggingConfig : ScriptableObject
    {
        public bool isEnabled = true;
        public string fileName = "log.txt";
        public int queueSize = 1000;
        public bool appendMode = true;
        public bool snapshotMode = false;
        public LogTypeFlags logFilter = LogTypeFlags.Log | LogTypeFlags.Warning | LogTypeFlags.Error | LogTypeFlags.Exception | LogTypeFlags.Assert;
        public LogDetailLevel detailLevel = LogDetailLevel.Text;

        private static RecordLoggingConfig _instance;
        public static RecordLoggingConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<RecordLoggingConfig>("RecordLoggingConfig");
                return _instance;
            }
        }
    }

    public enum LogDetailLevel
    {
        Text,
        Detail,
    }

    [System.Flags]
    public enum LogTypeFlags
    {
        Log = 1 << 0,
        Warning = 1 << 1,
        Error = 1 << 2,
        Exception = 1 << 3,
        Assert = 1 << 4,
    }
}
