using UnityEngine;
using UnityEngine.Events;

namespace TileMatch3.Core
{
    public class GlobalGameData : ScriptableObject
    {
        public UnityEvent onGameLose;
        public UnityEvent onGameWin;
    }
}