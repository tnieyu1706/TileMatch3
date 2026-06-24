using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;

namespace TnieYuPackage.Utils
{
    public static class PlayerLoopUtils
    {
        #region Remove

        public static void RemoveSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove)
        {
            if (loop.subSystemList == null) return;

            var playerLoopSystemList = new List<PlayerLoopSystem>(loop.subSystemList);
            for (int i = 0; i < playerLoopSystemList.Count; ++i)
            {
                if (playerLoopSystemList[i].type == systemToRemove.type &&
                    playerLoopSystemList[i].updateDelegate == systemToRemove.updateDelegate)
                {
                    playerLoopSystemList.RemoveAt(i);
                    loop.subSystemList = playerLoopSystemList.ToArray();
                    return;
                }
            }

            HandleSubSystemLoopForRemoval<T>(ref loop, systemToRemove);
        }

        static void HandleSubSystemLoopForRemoval<T>(ref PlayerLoopSystem loop, PlayerLoopSystem systemToRemove)
        {
            if (loop.subSystemList == null) return;

            for (int i = 0; i < loop.subSystemList.Length; ++i)
            {
                RemoveSystem<T>(ref loop.subSystemList[i], systemToRemove);
            }
        }

        #endregion

        #region Insert

        public static bool InsertSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
        {
            if (loop.type != typeof(T)) return HandleSubSystemLoop<T>(ref loop, systemToInsert, index);

            var playerLoopSystemList = new List<PlayerLoopSystem>();
            if (loop.subSystemList != null) playerLoopSystemList.AddRange(loop.subSystemList);
            playerLoopSystemList.Insert(Mathf.Min(index, playerLoopSystemList.Count), systemToInsert);
            loop.subSystemList = playerLoopSystemList.ToArray();
            return true;
        }

        static bool HandleSubSystemLoop<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
        {
            if (loop.subSystemList == null) return false;

            for (int i = 0; i < loop.subSystemList.Length; ++i)
            {
                if (!InsertSystem<T>(ref loop.subSystemList[i], in systemToInsert, index)) continue;
                return true;
            }

            return false;
        }

        #endregion

        #region Print

        [MenuItem("Tools/TnieYu/Print PlayerLoop")]
        private static void PrintCurrentPlayerLoop()
        {
            PlayerLoopSystem playerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
            PrintPlayerLoop(playerLoopSystem);
        }

        public static void PrintPlayerLoop(PlayerLoopSystem playerLoop)
        {
            StringBuilder sb = new();
            sb.AppendLine("PlayerLoopSystem:");
            foreach (PlayerLoopSystem subSystem in playerLoop.subSystemList)
            {
                PrintSubSystems(subSystem, sb, 0);
            }

            Debug.Log(sb.ToString());
        }

        static void PrintSubSystems(PlayerLoopSystem system, StringBuilder sb, int level)
        {
            if (level > 0) sb.Append('|');
            sb.Append('_', level * 2).AppendLine(system.type.ToString());
            if (system.subSystemList == null || system.subSystemList.Length == 0) return;

            foreach (PlayerLoopSystem subSystem in system.subSystemList)
            {
                PrintSubSystems(subSystem, sb, level + 1);
            }
        }

        #endregion
    }
}