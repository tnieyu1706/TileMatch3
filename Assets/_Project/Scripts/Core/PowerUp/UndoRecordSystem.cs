using System;
using System.Collections.Generic;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using TileMatch3.Core.BoardSystem;
using TileMatch3.Core.BoardSystem.Animations;
using TileMatch3.Core.Global;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.PowerUp
{
    /// <summary>
    /// Struct lưu trữ dữ liệu cần thiết của một bước đi, phục vụ cho Power-up Undo.
    /// Có thể mở rộng thêm biến nếu cần lưu layer hoặc order.
    /// </summary>
    public readonly struct TileUndoRecord
    {
        public TileRuntime Tile { get; }
        public Vector3 OriginalPosition { get; }

        public TileUndoRecord(TileRuntime tile, Vector3 originalPosition)
        {
            Tile = tile;
            OriginalPosition = originalPosition;
        }
    }

    public class UndoRecordSystem : MonoBehaviour
    {
        [Inject] private BoardController boardController;
        [Inject] private RackController rackController;
        [Inject] private GlobalGameplayDataVariable globalData;

        [Header("Dependencies")] [SerializeField]
        private TileMoveAnimatorSO tileMoveAnimator;

        [SerializeField] private float undoMoveDuration = 0.25f;

        // Dùng List chứa Record thay vì chỉ TileRuntime như trước
        private readonly List<TileUndoRecord> moveHistory = new List<TileUndoRecord>();

        public event Action<TileUndoRecord, TileRuntime> onTileUndone;
        public event Action<int> onMoveHistoryCountChanged;
        
        public int MoveHistoryCount => moveHistory.Count;

        private void Start()
        {
            boardController.onTileClick += RecordMove;
            rackController.onTileMergedData += RemoveMergedTilesFromRecord;
            globalData.Value.onPlayGame.AddListener(ClearRecord);

            onTileUndone += ExecuteTileUndone;
        }

        private void ExecuteTileUndone(TileUndoRecord tileRecord, TileRuntime tileRuntime)
        {
            int layer = Mathf.Abs(Mathf.RoundToInt(tileRecord.OriginalPosition.z));
            tileRuntime.SetSortingOrder(layer);
        }

        private void OnDestroy()
        {
            onTileUndone -= ExecuteTileUndone;

            if (boardController != null) boardController.onTileClick -= RecordMove;
            if (rackController != null) rackController.onTileMergedData -= RemoveMergedTilesFromRecord;
            if (globalData != null) globalData.Value.onPlayGame.RemoveListener(ClearRecord);
        }

        private void RecordMove(TileRuntime tile)
        {
            // Bắt và lưu lại Position ban đầu trước khi Tile bị di chuyển đi
            moveHistory.Add(new TileUndoRecord(tile, tile.transform.position));
            onMoveHistoryCountChanged?.Invoke(moveHistory.Count);
        }

        private void RemoveMergedTilesFromRecord(TileRuntime[] mergedTiles)
        {
            foreach (var tile in mergedTiles)
            {
                // Tìm kiếm record dựa trên tham chiếu Tile
                int index = moveHistory.FindIndex(r => r.Tile == tile);
                if (index != -1)
                {
                    moveHistory.RemoveAt(index);
                }
            }

            onMoveHistoryCountChanged?.Invoke(moveHistory.Count);
        }

        private void ClearRecord(int level)
        {
            moveHistory.Clear();
            onMoveHistoryCountChanged?.Invoke(moveHistory.Count);
        }

        [Button]
        public async void ExecuteUndo()
        {
            if (moveHistory.Count == 0)
            {
                Debug.Log("Không có bước nào để Undo!");
                return;
            }

            int lastIndex = moveHistory.Count - 1;
            TileUndoRecord lastRecord = moveHistory[lastIndex];

            TileRuntime poppedTile = rackController.Pop(lastRecord.Tile.TileId);
            
            moveHistory.RemoveAt(lastIndex);
            onMoveHistoryCountChanged?.Invoke(moveHistory.Count);

            if (poppedTile != null)
            {
                // Trả Data vào Core logic của Board
                boardController.SetTileToBoard(poppedTile, true);

                // Kích hoạt Data qua SO Animator để Tile bay ngược về vị trí cũ trên Board
                if (tileMoveAnimator != null)
                {
                    await tileMoveAnimator.MoveTile(poppedTile, lastRecord.OriginalPosition, undoMoveDuration);
                }

                onTileUndone?.Invoke(lastRecord, poppedTile);
            }
            else
            {
                Debug.LogWarning("Tile không còn tồn tại trên Rack để Undo, xóa khỏi lịch sử.");
            }
        }

        public bool HasMovesToUndo() => moveHistory.Count > 0;
    }
}