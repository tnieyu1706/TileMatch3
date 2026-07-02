using Reflex.Attributes;
using UnityEngine;

namespace TileMatch3.Core.PowerUp
{
    [DefaultExecutionOrder(-5)]
    public class UndoPower : BasePower
    {
        [Inject] private UndoRecordSystem undoRecordSystem;
        
        private void Start()
        {
            OnUndoHistoryCountChanged(undoRecordSystem.MoveHistoryCount);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            undoRecordSystem.onMoveHistoryCountChanged += OnUndoHistoryCountChanged;
        }
        
        protected override void OnButtonClicked()
        {
            base.OnButtonClicked();
            undoRecordSystem.ExecuteUndo();
        }

        private void OnUndoHistoryCountChanged(int count)
        {
            bool isUndoPossible = count > 0;
            IsDoubleCheck = isUndoPossible;

            if (IsEnoughEnable && isUndoPossible)
            {
                SetPowerOn();
            }
            else
            {
                SetPowerOff();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            undoRecordSystem.onMoveHistoryCountChanged -= OnUndoHistoryCountChanged;
        }
    }
}