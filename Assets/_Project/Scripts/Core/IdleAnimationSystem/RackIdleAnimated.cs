using System.Collections.Generic;
using LitMotion;
using Reflex.Attributes;
using TileMatch3.Core.BoardSystem;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.IdleAnimationSystem
{
    public class RackIdleAnimated : MonoBehaviour, IIdleAnimated
    {
        [Header("Animation Settings")] [SerializeField]
        private float jumpHeight = 0.3f; // Độ cao khi nhảy lên

        [SerializeField] private float duration = 0.5f; // Tổng thời gian (lên + xuống)
        [SerializeField] private Ease ease = Ease.OutQuad; // OutQuad tạo cảm giác trọng lực rất tốt khi nhảy

        [Inject] private RackController rackController;

        private MotionHandle rackMotionHandle;
        private Vector3 rackDefaultPosition;

        // Quản lý trạng thái và CHỈ LƯU vị trí trục Y gốc của từng viên Tile để không đè trục X, Z khi có Collapse
        private readonly Dictionary<TileRuntime, (MotionHandle handle, float defaultY)> animatingTiles = new();

        public void Play()
        {
            // --- 1. Xử lý hoạt ảnh nảy lên cho thanh Rack (Tạm đóng theo code cũ của bạn) ---
            // if (rackMotionHandle.IsActive())
            // {
            //     rackMotionHandle.Cancel();
            //     transform.localPosition = rackDefaultPosition;
            // }
            // else
            // {
            //     // Bắt vị trí gốc khi đang đứng yên
            //     rackDefaultPosition = transform.localPosition;
            // }
            //
            // Vector3 rackTargetPosition = rackDefaultPosition + new Vector3(0, jumpHeight, 0);
            //
            // rackMotionHandle = LMotion.Create(rackDefaultPosition, rackTargetPosition, duration / 2f)
            //     .WithEase(ease)
            //     .WithLoops(2, LoopType.Yoyo)
            //     .BindToLocalPosition(transform);


            // --- 2. Xử lý hoạt ảnh nảy lên cho các viên Tile (Active) trên Rack ---
            // Huỷ toàn bộ các hoạt ảnh Tile cũ nếu còn chạy lấn sang nhịp mới
            CancelAllTileMotions();

            List<TileRuntime> activeTiles = rackController.GetActiveRackTiles();
            foreach (var tile in activeTiles)
            {
                if (tile == null) continue;

                // Chỉ lưu giữ lại trục Y nguyên bản trước khi nhảy
                float defaultY = tile.transform.position.y;

                // Tạo animation thay đổi giá trị offset của Y từ 0 lên jumpHeight
                var handle = LMotion.Create(0f, jumpHeight, duration / 2f)
                    .WithEase(ease)
                    .WithLoops(2, LoopType.Yoyo)
                    .Bind(yOffset =>
                    {
                        // RẤT QUAN TRỌNG: Kiểm tra null an toàn
                        if (tile != null && tile.gameObject != null)
                        {
                            Vector3 currentPos = tile.transform.position;
                            // Chỉ cập nhật Y, giữ nguyên X và Z hiện hành (nhờ vậy Collapse không bị ảnh hưởng)
                            tile.transform.position = new Vector3(currentPos.x, defaultY + yOffset, currentPos.z);
                        }
                    });

                // Lưu lại thông tin để có thể Cancel và Reset sau này
                animatingTiles[tile] = (handle, defaultY);
            }
        }

        private void CancelAllTileMotions()
        {
            foreach (var kvp in animatingTiles)
            {
                TileRuntime tile = kvp.Key;
                var data = kvp.Value;

                if (data.handle.IsActive())
                {
                    data.handle.Cancel();

                    // Trả lại vị trí Y gốc, giữ nguyên X và Z
                    if (tile != null && tile.gameObject != null)
                    {
                        Vector3 currentPos = tile.transform.position;
                        tile.transform.position = new Vector3(currentPos.x, data.defaultY, currentPos.z);
                    }
                }
            }

            animatingTiles.Clear();
        }

        private void OnDestroy()
        {
            // Dọn dẹp memory khi huỷ object
            if (rackMotionHandle.IsActive())
            {
                rackMotionHandle.Cancel();
            }

            CancelAllTileMotions();
        }
    }
}