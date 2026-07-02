using System;
using System.Collections.Generic;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using TileMatch3.Core.BoardSystem;
using TileMatch3.Core.EffectSystem.Commands;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.Combo
{
    [Serializable]
    public struct ComboInfo
    {
        public string text;
        public Color color;
        public SFXCommand sfxCommand;
    }

    public class ComboSystem : MonoBehaviour
    {
        [SerializeField] private ComboDisplayer comboDisplayer;
        [SerializeField] private int limitComboBrokeNumber = 3;
        [SerializeField] private float displayDuration = 1f;
        [SerializeField] private List<ComboInfo> comboInfos = new();

        [Inject] private RackController rackController;

        [SerializeField, ReadOnly]
        private int currentComboIndex;
        [SerializeField, ReadOnly]
        private int noComboCount;

        private void Start()
        {
            ResetComboIndex();
            ResetNoComboCount();
        }

        private void ResetNoComboCount()
        {
            noComboCount = 0;
        }
        
        private void ResetComboIndex()
        {
            currentComboIndex = -1;
        }

        private void OnEnable()
        {
            if (rackController != null)
            {
                rackController.OnTilePushedData += CheckComboHasBroke;
                rackController.onTileMergedData += DisplayComboWhenTileMerged;
            }
        }

        private void CheckComboHasBroke(TileRuntime _)
        {
            noComboCount++;
            if (noComboCount > limitComboBrokeNumber)
            {
                ResetComboIndex();
            }
        }

        private void DisplayComboWhenTileMerged(TileRuntime[] _)
        {
            ResetNoComboCount();
            if (currentComboIndex < 0)
            {
                currentComboIndex++;
                return;
            }

            var greaterComboIndex = currentComboIndex - comboInfos.Count;
            ComboInfo comboInfo;
            if (greaterComboIndex < 0)
            {
                comboInfo = comboInfos[currentComboIndex];
            }
            else
            {
                comboInfo = comboInfos[^1];
                comboInfo.text += $" x{greaterComboIndex + 1}";
            }

            // Hiển thị comboInfo.text và comboInfo.color lên UI hoặc thực hiện các hành động khác liên quan đến combo
            comboDisplayer.DisplayCombo(comboInfo, displayDuration).Forget();

            currentComboIndex++;
        }

        private void OnDisable()
        {
            if (rackController != null)
            {
                rackController.OnTilePushedData -= CheckComboHasBroke;
                rackController.onTileMergedData -= DisplayComboWhenTileMerged;
            }
        }
    }
}