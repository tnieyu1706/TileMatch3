using System.Collections.Generic;
using UnityEngine;
using TileMatch3.Core.Tile;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using TileMatch3.Core.BoardSystem.Animations;

namespace TileMatch3.Core.BoardSystem
{
    public class RackVisualController : MonoBehaviour
    {
        [Inject] private RackController rackController;
        [SerializeField] private float moveDuration = 0.25f;
        [SerializeField] private float mergeDuration = 0.25f;

        [Header("Animation Strategies")]
        // Sử dụng List với SerializeReference để Odin hiển thị danh sách đa hình
        [SerializeReference]
        private List<IMoveAnimStrategy> moveStrategies = new();

        [SerializeReference] private List<IMergeAnimStrategy> mergeStrategies = new();

        private void OnEnable()
        {
            rackController.onTileMoving += HandleTileMoving;
            rackController.onTileMerged += HandleTileMerged;
        }

        private void OnDisable()
        {
            rackController.onTileMoving -= HandleTileMoving;
            rackController.onTileMerged -= HandleTileMerged;
        }

        private void HandleTileMoving(TileRuntime tile, Vector2 targetPos, bool isPlayEffect)
        {
            if (moveStrategies != null && moveStrategies.Count > 0)
            {
                // Chọn ngẫu nhiên 1 strategy từ danh sách
                var randomStrategy = moveStrategies[UnityEngine.Random.Range(0, moveStrategies.Count)];
                randomStrategy.PlayMoveAnimation(tile, targetPos, moveDuration, isPlayEffect).Forget();
            }
        }

        private async UniTask HandleTileMerged(TileRuntime[] mergedTiles)
        {
            // Mất moveDuration giây đầu tiên để các tile bay ổn định vào Rack
            await UniTask.Delay(System.TimeSpan.FromSeconds(moveDuration));

            if (mergeStrategies != null && mergeStrategies.Count > 0)
            {
                // Chọn ngẫu nhiên 1 strategy để merge
                var randomStrategy = mergeStrategies[Random.Range(0, mergeStrategies.Count)];
                await randomStrategy.PlayMergeAnimation(mergedTiles, mergeDuration);
            }
        }
    }
}