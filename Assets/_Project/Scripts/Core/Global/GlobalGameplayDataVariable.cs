using System;
using Obvious.Soap;
using UnityEngine;
using UnityEngine.Events;

namespace TileMatch3.Core.Global
{
    [Serializable]
    public class GlobalGameplayData
    {
        public UnityEvent onGameWin;
        public UnityEvent onGameLose;
        public UnityEvent<int> onPlayGame;

        public int level;
    }

    [CreateAssetMenu(menuName = "TileMatch3/Global/GlobalGameplayDataVariable")]
    public class GlobalGameplayDataVariable : ScriptableVariable<GlobalGameplayData>
    {
        public override void Save()
        {
            PlayerPrefs.SetInt(TileMatch3Constraint.GetKey(Guid), Value.level);
            base.Save();
        }

        public override void Load()
        {
            Value.level = PlayerPrefs.GetInt(TileMatch3Constraint.GetKey(Guid), _saved ? DefaultValue.level : 1);
            base.Load();
        }
    }
}