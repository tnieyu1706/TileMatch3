using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Flexalon;
using JetBrains.Annotations;
using Reflex.Attributes;
using TileMatch3.Core.Global;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.BoardSystem
{
    public class RackController : MonoBehaviour
    {
        [Inject] private GlobalGameplayDataVariable dataVariable;
        [SerializeField] private FlexalonObject slotsFlexalon;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Vector2 offset = new(0, 0.1f);

        private int slotNumber = 7;

        private Vector2[] tilePositions;

        // Dùng List thay cho Array để xử lý linh hoạt việc tràn slot ảo (khi tile đang đợi animation merge)
        private readonly List<TileRuntime> rackTiles = new List<TileRuntime>();

        // Cache lưu trữ các tile ĐANG TRONG QUÁ TRÌNH MERGE
        private readonly List<TileRuntime> mergingTiles = new List<TileRuntime>();

        public event Action<TileRuntime, Vector2, bool> onTileMoving;
        public event Func<TileRuntime[], UniTask> onTileMerged;

        public Transform SlotRootTransform => slotsFlexalon.transform;

        private void Awake()
        {
            // TODO: register related handler when board onWin, onLose
        }

        private void OnDestroy()
        {
            // TODO: un-register related handler when board onWin, onLose
        }

        // Tính toán vị trí ảo cho các index vượt quá slotNumber
        private Vector2 GetPositionForIndex(int index)
        {
            if (index < slotNumber)
            {
                return tilePositions[index];
            }
            else
            {
                // Fake position: Tính khoảng cách giữa 2 slot đầu để ngoại suy vị trí cho các slot tràn
                if (slotNumber >= 2)
                {
                    Vector2 distance = tilePositions[1] - tilePositions[0];
                    return tilePositions[0] + distance * index;
                }

                return Vector2.zero;
            }
        }

        private int SpecifyInsertSlot(Guid tileId)
        {
            int insertIndex = rackTiles.Count;
            for (int i = 0; i < rackTiles.Count; i++)
            {
                if (rackTiles[i].TileId == tileId)
                {
                    insertIndex = i + 1;
                }
            }

            return insertIndex;
        }

        private void CheckAndMerge()
        {
            int checkIndex = 0;
            for (int i = 1; i < rackTiles.Count; i++)
            {
                if (rackTiles[i].TileId != rackTiles[checkIndex].TileId)
                {
                    checkIndex = i;
                }
                else if (i - checkIndex == 2) // Tìm thấy bộ 3
                {
                    // Kiểm tra xem bộ 3 này đã nằm trong cache merging chưa (tránh trigger 2 lần)
                    bool isAlreadyMerging = false;
                    for (int j = checkIndex; j <= i; j++)
                    {
                        if (mergingTiles.Contains(rackTiles[j]))
                        {
                            isAlreadyMerging = true;
                            break;
                        }
                    }

                    if (!isAlreadyMerging)
                    {
                        MergeTilesAsync(checkIndex, i).Forget();
                        return; // Xử lý 1 lần merge trước, nếu có nhiều bộ 3 sẽ check tiếp ở lần Push sau
                    }
                }
            }

            if (IsRackFull())
            {
                OnRackFull();
            }
        }

        private async UniTaskVoid MergeTilesAsync(int startIndex, int endIndex)
        {
            int range = endIndex - startIndex + 1; // 3
            TileRuntime[] mergeRange = new TileRuntime[range];

            for (int i = 0; i < range; i++)
            {
                mergeRange[i] = rackTiles[startIndex + i];
                // Đưa vào cache ngay lập tức để không bị tính là slot đang chiếm dụng
                mergingTiles.Add(mergeRange[i]);
            }

            // Bắn event và CHỜ đợi UI Animation Merge chạy xong
            if (onTileMerged != null)
            {
                await onTileMerged.Invoke(mergeRange);
            }

            // Release merge tiles
            foreach (var tile in mergeRange)
            {
                // Reset lại Scale trước khi trả về Pool để lần sau spawn ra không bị tàng hình
                tile.transform.localScale = Vector3.one;
                tile.transform.localRotation = Quaternion.identity;

                if (tile.Pool != null)
                    tile.Pool.Release(tile);
                else
                    Destroy(tile.gameObject);
            }

            // --- Sau khi Animation UI xong, mới xóa khỏi data và Collapse (dồn trái) ---
            foreach (var tile in mergeRange)
            {
                rackTiles.Remove(tile);
                mergingTiles.Remove(tile);
            }

            // Cập nhật lại vị trí cho tất cả các Tile còn lại trên Rack (Tạo hiệu ứng Collapse)
            for (int i = 0; i < rackTiles.Count; i++)
            {
                onTileMoving?.Invoke(rackTiles[i], GetPositionForIndex(i), false);
            }

            // Kiểm tra lại xem sau khi merge và collapse xong, rack có bị đầy không
            if (IsRackFull())
            {
                OnRackFull();
            }
        }

        private void OnRackFull()
        {
            dataVariable?.Value.onGameLose?.Invoke();
        }

        public bool IsRackFull()
        {
            int logicalCount = 0;
            foreach (var tile in rackTiles)
            {
                // Các tile đang merge sẽ tàng hình đối với logic đếm sức chứa
                if (!mergingTiles.Contains(tile))
                {
                    logicalCount++;
                }
            }

            return logicalCount >= slotNumber;
        }

        public void Push(TileRuntime tileRuntime)
        {
            tileRuntime.isOnRack = true;

            int index = SpecifyInsertSlot(tileRuntime.TileId);

            // Insert đẩy các tile đằng sau lùi lại 1 index
            rackTiles.Insert(index, tileRuntime);

            // Cập nhật vị trí UI cho tile mới và các tile bị đẩy sang phải
            onTileMoving?.Invoke(rackTiles[index], GetPositionForIndex(index), true);
            for (int i = index + 1; i < rackTiles.Count; i++)
            {
                onTileMoving?.Invoke(rackTiles[i], GetPositionForIndex(i), false);
            }

            CheckAndMerge();
        }

        [CanBeNull]
        public TileRuntime Pop(Guid guid) // Thường dùng cho tính năng Undo
        {
            int index = -1;
            // Tìm từ cuối lên, KHÔNG lấy các tile đang trong quá trình merge
            for (int i = rackTiles.Count - 1; i >= 0; i--)
            {
                if (rackTiles[i].TileId == guid && !mergingTiles.Contains(rackTiles[i]))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1) return null;

            var poppedTile = rackTiles[index];
            rackTiles.RemoveAt(index);

            // Dịch các tile bên phải sang trái
            for (int i = index; i < rackTiles.Count; i++)
            {
                onTileMoving?.Invoke(rackTiles[i], GetPositionForIndex(i), false);
            }

            return poppedTile;
        }

        public async void Setup(int quantity, float scale)
        {
            slotNumber = quantity;

            // TODO: Clear rack
            ClearRackState();
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

            for (var i = 0; i < quantity; i++)
            {
                SlotRootTransform.GetChild(i).gameObject.SetActive(true);
            }

            await UniTask.NextFrame(); // Đợi 1 frame cho Flexalon layout xếp xong

            for (var i = 0; i < quantity; i++)
            {
                tilePositions[i] = SlotRootTransform.GetChild(i).transform.position + (Vector3)offset * scale;
            }
        }

        private void ClearRackState()
        {
            foreach (var tile in rackTiles)
            {
                if (tile.Pool != null)
                    tile.Pool.Release(tile);
                else
                    Destroy(tile.gameObject);
            }

            rackTiles.Clear();
            mergingTiles.Clear();
        }
    }
}