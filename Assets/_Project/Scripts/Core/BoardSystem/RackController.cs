using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.BoardSystem
{
    public class RackController : MonoBehaviour
    {
        [SerializeField] private Transform rackRoot;
        private int slotNumber = 7;

        private Vector2[] tilePositions;
        private TileRuntime[] rackTiles;

        public event Action<int, float, float> onRackSetup;
        public event Action onRackFull;
        
        public event Action<TileRuntime, Vector2> onTileMoving; 
        
        // 3. Sử dụng Func trả về UniTask để Rack chờ UI xử lý xong mới collapse
        public event Func<TileRuntime[], UniTask> onTileMerged;

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

            // 3. Bắn event và CHỜ đợi UI Animation Merge chạy xong (Tile thu nhỏ biến mất)
            if (onTileMerged != null)
            {
                await onTileMerged.Invoke(mergeRange);
            }

            // --- Sau khi await xong, các Tile ở Rack mới tiến hành Collapse (dồn trái) ---
            var endMoveIndex = slotNumber - range;
            for (int i = startIndex; i < endMoveIndex; i++)
            {
                rackTiles[i] = rackTiles[i + range];
                if (rackTiles[i] != null)
                {
                    onTileMoving?.Invoke(rackTiles[i], tilePositions[i]);
                }
            }

            for (int i = endMoveIndex; i < slotNumber; i++)
            {
                rackTiles[i] = null;
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

        public void Setup(int rackSlotNumber, float tileSize, float gap)
        {
            slotNumber = rackSlotNumber;
            Vector2 rackPosition = rackRoot.transform.position;

            rackTiles = new TileRuntime[slotNumber];
            tilePositions = new Vector2[slotNumber];

            onRackSetup?.Invoke(rackSlotNumber, tileSize, gap);

            float eachWidth = tileSize + gap;
            float startX = rackPosition.x - (eachWidth * ((float)rackSlotNumber / 2f)) + (eachWidth / 2f);

            for (int i = 0; i < rackSlotNumber; i++)
            {
                tilePositions[i] = new Vector2(startX + (eachWidth * i), rackPosition.y);
            }
        }
    }
}