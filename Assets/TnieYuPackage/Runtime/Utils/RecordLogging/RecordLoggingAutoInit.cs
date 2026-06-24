using UnityEngine;

namespace TnieYuPackage.Utils
{
    public static class RecordLoggingAutoInit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit()
        {
            var config = RecordLoggingConfig.Instance;
            if (config != null && config.isEnabled)
            {
                RecordLoggingService.Initialize(config);
            }
        }
    }
}
