using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using TileMatch3.Core.EffectSystem.Commands;
using UnityEngine;

namespace TileMatch3.Core.Tile
{
    [RequireComponent(typeof(TileRuntime))]
    public class TileVisualComponent : MonoBehaviour
    {
        [SerializeField] private TileRuntime tileRuntime;

        [Header("Invalid Click Animation")] [Tooltip("Góc lắc dội (độ)")] [SerializeField]
        private float wobbleAngle = 15f;

        [SerializeField] private float wobbleDuration = 0.35f;

        [SerializeReference] private List<IEffectCommand> onInvalidClickEffects = new List<IEffectCommand>();

        private bool isAnimating = false;
        private Quaternion originalRotation;

        private void Awake()
        {
            if (tileRuntime == null) tileRuntime = GetComponent<TileRuntime>();

            // LƯU Ý QUAN TRỌNG: Chỉ lưu rotation gốc 1 lần duy nhất lúc khởi tạo
            originalRotation = transform.localRotation;

            if (tileRuntime != null)
            {
                tileRuntime.OnTileNotAllowClicked += HandleInvalidClick;
            }
        }

        private void OnDestroy()
        {
            if (tileRuntime != null)
            {
                tileRuntime.OnTileNotAllowClicked -= HandleInvalidClick;
            }
        }

        private void HandleInvalidClick(TileRuntime tile)
        {
            // BẮT BUỘC PHẢI CHẶN: Nếu đang chạy anim mà click tiếp thì bỏ qua
            // Để tránh việc anim đè lên nhau gây hỏng transform
            if (isAnimating) return;

            PlayInvalidClickAnimAsync(tile).Forget();
        }

        private async UniTaskVoid PlayInvalidClickAnimAsync(TileRuntime tile)
        {
            isAnimating = true;

            // 1. Chạy Effects (SFX / VFX)
            if (onInvalidClickEffects != null)
            {
                foreach (var cmd in onInvalidClickEffects)
                {
                    cmd.Execute(transform.position, wobbleDuration, tile.MainColor);
                }
            }

            // 2. Chạy Animation Lắc (Wobble) bằng hàm Punch tích hợp của LitMotion
            // Punch sẽ đẩy giá trị từ 0 lên wobbleAngle rồi tự dội ngược về 0 (như hiệu ứng lò xo)
            await LMotion.Punch.Create(0f, wobbleAngle, wobbleDuration)
                .WithEase(Ease.OutQuad)
                .Bind(x => transform.localRotation = originalRotation * Quaternion.Euler(0, 0, x))
                .ToUniTask();

            // Đảm bảo set lại vị trí tuyệt đối sau khi xong
            transform.localRotation = originalRotation;
            isAnimating = false;
        }
    }
}