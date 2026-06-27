using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Flexalon;
using JetBrains.Annotations;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.BoardSystem
{
    public class RackController : MonoBehaviour
    {
        [SerializeField] private FlexalonObject slotsFlexalon;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Vector2 offset = new(0, 0.1f);

        private int slotNumber = 7;

        private Vector2[] tilePositions;
        private TileRuntime[] rackTiles;

        public event Action onRackFull;

        public event Action<TileRuntime, Vector2> onTileMoving;

        // 3. Sử dụng Func trả về UniTask để Rack chờ UI xử lý xong mới collapse
        public event Func<TileRuntime[], UniTask> onTileMerged;

        public Transform SlotRootTransform => slotsFlexalon.transform;

        private int SpecifyInsertSlot(Guid tileId)
        {
            int insertIndex = -1;
            for (int i = 0; i < slotNumber; i++)
            {
                if (rackTiles[i] == null)
                {
                    if (insertIndex == -1) insertIndex = i;
                    break;
                }

                if (tileId == rackTiles[i].TileId)
                {
                    insertIndex = i + 1;
                }
            }

            return Mathf.Clamp(insertIndex, 0, slotNumber - 1);
        }

        private void RefreshRackState()
        {
            int checkIndex = 0;
            for (int i = 1; i < slotNumber; i++)
            {
                if (rackTiles[i] == null) break;

                if (rackTiles[i].TileId != rackTiles[checkIndex].TileId)
                {
                    checkIndex = i;
                }
                else if (i - checkIndex == 2)
                {
                    // Chạy Async nhưng không block thread chính
                    MergeTilesAsync(checkIndex, i + 1).Forget();
                    return;
                }
            }

            if (IsRackFull())
            {
                onRackFull?.Invoke();
            }
        }

        private async UniTaskVoid MergeTilesAsync(int startIndex, int endIndex)
        {
            int range = endIndex - startIndex; // 3

            TileRuntime[] mergeRange = new TileRuntime[range];
            for (int i = 0; i < range; i++)
            {
                mergeRange[i] = rackTiles[startIndex + i];
            }

            // --- Xử lí dữ liệu các Tile ở Rack mới tiến hành Collapse (dồn trái) ---
            var endMoveIndex = slotNumber - range;
            List<(TileRuntime, Vector2)> movedTiles = new();
            for (int i = startIndex; i < endMoveIndex; i++)
            {
                rackTiles[i] = rackTiles[i + range];
                if (rackTiles[i] != null)
                {
                    movedTiles.Add((rackTiles[i], tilePositions[i]));
                }
            }

            for (int i = endMoveIndex; i < slotNumber; i++)
            {
                rackTiles[i] = null;
            }

            // Bắn event và CHỜ đợi UI Animation Merge chạy xong (Tile thu nhỏ biến mất)
            if (onTileMerged != null)
            {
                await onTileMerged.Invoke(mergeRange);
            }

            foreach (var tileMove in movedTiles)
            {
                onTileMoving?.Invoke(tileMove.Item1, tileMove.Item2);
            }
        }

        public bool IsRackFull()
        {
            if (rackTiles == null) return false;
            foreach (var item in rackTiles)
            {
                if (item == null) return false;
            }

            return true;
        }

        public void Push(TileRuntime tileRuntime)
        {
            tileRuntime.isOnRack = true;

            int index = SpecifyInsertSlot(tileRuntime.TileId);
            ShiftRightOne(index);

            rackTiles[index] = tileRuntime;
            onTileMoving?.Invoke(tileRuntime, tilePositions[index]);

            RefreshRackState();
        }

        private void ShiftRightOne(int index)
        {
            if (index >= slotNumber - 1) return;

            for (int i = slotNumber - 1; i > index; i--)
            {
                rackTiles[i] = rackTiles[i - 1];
                if (rackTiles[i] != null)
                {
                    onTileMoving?.Invoke(rackTiles[i], tilePositions[i]);
                }
            }
        }

        [CanBeNull]
        public TileRuntime Pop(Guid guid)
        {
            int index = -1;
            for (int i = 0; i < slotNumber; i++)
            {
                if (rackTiles[i] != null && rackTiles[i].TileId == guid)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1) return null;

            var poppedTile = rackTiles[index];

            for (int i = index; i < slotNumber - 1; i++)
            {
                rackTiles[i] = rackTiles[i + 1];
                if (rackTiles[i] != null)
                {
                    onTileMoving?.Invoke(rackTiles[i], tilePositions[i]);
                }
            }

            rackTiles[slotNumber - 1] = null;
            return poppedTile;
        }

        public async void Setup(int quantity, float scale)
        {
            slotNumber = quantity;
            rackTiles = new TileRuntime[slotNumber];
            tilePositions = new Vector2[slotNumber];

            slotsFlexalon.Scale = Vector2.one * scale;

            var slotChildren = SlotRootTransform.childCount;
            if (slotChildren > quantity)
            {
                for (int i = quantity; i < slotChildren; i++)
                {
                    SlotRootTransform.GetChild(i).gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = slotChildren; i < quantity; i++)
                {
                    Instantiate(slotPrefab, SlotRootTransform);
                }
            }

            // TODO: active all slot (0 -> quantity - 1) to re-calculate layout and set tilePositions
            for (var i = 0; i < quantity; i++)
            {
                SlotRootTransform.GetChild(i).gameObject.SetActive(true);
            }

            await UniTask.NextFrame(); // Wait for one frame to ensure layout is updated

            for (var i = 0; i < quantity; i++)
            {
                tilePositions[i] = SlotRootTransform.GetChild(i).transform.position + (Vector3)offset * scale;
            }
        }
    }
}