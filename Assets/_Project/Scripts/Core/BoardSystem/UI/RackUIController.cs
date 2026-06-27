using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using TileMatch3.Core.Tile;
using Cysharp.Threading.Tasks;

namespace TileMatch3.Core.BoardSystem
{
    public class RackUIController : MonoBehaviour
    {
        [SerializeField] private RackController rackController;
        [SerializeField] private float moveDuration = 0.2f;
        [SerializeField] private float mergeDuration = 0.2f;

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

        private void HandleTileMoving(TileRuntime tile, Vector2 targetPos)
        {
            tile.SetSortingOrder(100); 

            LMotion.Create(tile.transform.position, new Vector3(targetPos.x, targetPos.y, tile.transform.position.z), moveDuration)
                .WithEase(Ease.OutQuad)
                .BindToPosition(tile.transform);
        }

        private async UniTask HandleTileMerged(TileRuntime[] mergedTiles)
        {
            // Mất moveDuration giây đầu tiên để các tile ổn định vị trí trên Rack nếu nó đang bay từ Board xuống
            await UniTask.Delay(System.TimeSpan.FromSeconds(moveDuration));

            foreach (var tile in mergedTiles)
            {
                LMotion.Create(tile.transform.localScale, Vector3.zero, mergeDuration)
                    .WithEase(Ease.InBack)
                    .WithOnComplete(() =>
                    {
                        if (tile.Pool != null) 
                            tile.Pool.Release(tile); 
                        else 
                            Destroy(tile.gameObject);
                    })
                    .BindToLocalScale(tile.transform);
            }

            // Chờ animation thu nhỏ hoàn tất
            await UniTask.Delay(System.TimeSpan.FromSeconds(mergeDuration));
        }
    }
}