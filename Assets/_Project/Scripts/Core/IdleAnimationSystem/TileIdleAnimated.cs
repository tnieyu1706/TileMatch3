using LitMotion;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.IdleAnimationSystem
{
    public class TileIdleAnimated : MonoBehaviour, IIdleAnimated
    {
        public TileRuntime tileRuntime;
        
        [Header("Animation Settings")] [SerializeField]
        private float wobbleAngle = 15f; // Góc nghiêng để lắc

        [SerializeField] private float duration = 0.8f; // Thời gian lắc
        [SerializeField] private Ease ease = Ease.OutElastic;

        private MotionHandle motionHandle;
        private Quaternion defaultRotation;

        public void Play()
        {
            // Nếu animation đang chạy, ta huỷ nó và reset về defaultRotation đã lưu trước đó
            if (motionHandle.IsActive())
            {
                motionHandle.Cancel();
                transform.localRotation = defaultRotation;
            }
            else
            {
                // CHỈ lưu lại rotation khi KHÔNG CÓ animation nào đang chạy.
                // Điều này giúp lấy đúng giá trị gốc an toàn ngay tại thời điểm gọi Play.
                defaultRotation = transform.localRotation;
            }

            // Mẹo tạo hiệu ứng lắc (Wobble/Jelly) cực mượt:
            // Đẩy nhẹ góc xoay Z tới wobbleAngle, sau đó dùng Ease.OutElastic để đàn hồi về 0
            transform.localEulerAngles = new Vector3(0, 0, wobbleAngle);

            motionHandle = LMotion.Create(wobbleAngle, 0f, duration)
                .WithEase(ease)
                .Bind(z =>
                {
                    // Check null để an toàn nếu Tile bị Pool thu hồi giữa chừng
                    if (this != null && transform != null)
                    {
                        transform.localEulerAngles = new Vector3(0, 0, z);
                    }
                });
        }

        private void OnDestroy()
        {
            // Dọn dẹp
            if (motionHandle.IsActive())
            {
                motionHandle.Cancel();
            }
        }

        // Thêm hàm OnDisable nếu Tile của bạn dùng Object Pool
        // để chắc chắn ngắt animation và trả góc xoay về mặc định khi thu hồi Tile về kho.
        private void OnDisable()
        {
            if (motionHandle.IsActive())
            {
                motionHandle.Cancel();
                transform.localRotation = defaultRotation;
            }
        }
    }
}