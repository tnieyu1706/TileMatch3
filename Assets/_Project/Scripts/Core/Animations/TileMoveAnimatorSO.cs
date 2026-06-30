using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.BoardSystem.Animations
{
    [CreateAssetMenu(fileName = "TileMoveAnimator", menuName = "TileMatch3/Animations/TileMoveAnimator")]
    public class TileMoveAnimatorSO : ScriptableObject
    {
        /// <summary>
        /// Hàm cốt lõi để di chuyển Tile (từ Board -> Rack, Rack -> Board).
        /// Trả về UniTask để nơi gọi có thể Await hoặc Forget() tùy ý.
        /// </summary>
        public async UniTask MoveTile(TileRuntime tile, Vector3 targetPos, float duration, Ease moveEase = Ease.OutCubic)
        {
            if (tile == null) return;

            // 1. Đảm bảo Tile luôn đè lên các tile khác khi đang bay
            tile.SetSortingOrder(100);

            // 2. Nội suy vị trí di chuyển qua LitMotion
            await LMotion.Create(tile.transform.position, targetPos, duration)
                .WithEase(moveEase)
                .BindToPosition(tile.transform)
                .ToUniTask();
        }
    }
}