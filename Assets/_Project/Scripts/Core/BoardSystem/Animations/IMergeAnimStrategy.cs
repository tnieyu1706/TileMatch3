using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TileMatch3.Core.Tile;
using Object = UnityEngine.Object;

namespace TileMatch3.Core.BoardSystem.Animations
{
    public interface IMergeAnimStrategy
    {
        UniTask PlayMergeAnimation(TileRuntime[] mergedTiles, float duration);
    }

    [Serializable]
    public class MergeToCenterStrategy : IMergeAnimStrategy
    {
        [Tooltip("Loại Ease khi thu nhỏ / gộp")]
        public Ease mergeEase = Ease.InBack;

        [Tooltip("Prefab hiệu ứng nổ (VFX) khi merge thành công")]
        public GameObject mergeVfxPrefab;

        public async UniTask PlayMergeAnimation(TileRuntime[] mergedTiles, float duration)
        {
            if (mergedTiles == null || mergedTiles.Length != 3) return;

            // Xác định tile ở giữa (Thường là phần tử thứ 1 trong mảng 3 phần tử)
            Vector3 centerPos = mergedTiles[1].transform.position;

            // Đẩy Z lên để không bị che khuất khi gom vào nhau
            centerPos.z -= 1f;

            List<UniTask> animationTasks = new List<UniTask>();

            foreach (var tile in mergedTiles)
            {
                // 1. Animation di chuyển tụ vào điểm giữa
                var moveTask = LMotion.Create(tile.transform.position, centerPos, duration)
                    .WithEase(Ease.InQuad)
                    .BindToPosition(tile.transform)
                    .ToUniTask();
                animationTasks.Add(moveTask);

                // 2. Animation thu nhỏ (Scale) về 0
                var scaleTask = LMotion.Create(tile.transform.localScale, Vector3.zero, duration)
                    .WithEase(mergeEase)
                    .BindToLocalScale(tile.transform)
                    .ToUniTask();
                animationTasks.Add(scaleTask);
            }

            // Chờ cho TẤT CẢ các hiệu ứng gom và thu nhỏ hoàn tất đồng thời
            await UniTask.WhenAll(animationTasks);

            // 3. Play VFX tại tâm điểm
            if (mergeVfxPrefab != null)
            {
                // Instantiate VFX (Có thể dùng Object Pool cho VFX sau này)
                GameObject vfx = Object.Instantiate(mergeVfxPrefab, centerPos, Quaternion.identity);
                // Giả định VFX có gắn script tự huỷ hoặc tự dùng ParticleSystem.Destroy
                Object.Destroy(vfx, 2f);
            }

            // 4. Dọn dẹp Data
            foreach (var tile in mergedTiles)
            {
                // Reset lại Scale trước khi trả về Pool để lần sau spawn ra không bị tàng hình
                tile.transform.localScale = Vector3.one;
                tile.transform.localRotation = Quaternion.identity;

                if (tile.Pool != null)
                    tile.Pool.Release(tile);
                else
                    Object.Destroy(tile.gameObject);
            }
        }
    }

    [Serializable]
    public class MergeAndFlyUpStrategy : IMergeAnimStrategy
    {
        [Tooltip("Prefab hiệu ứng VFX")] public GameObject mergeVfxPrefab;

        [Tooltip("Độ cao bay lên trước khi biến mất")]
        public float flyUpDistance = 1.5f;

        public async UniTask PlayMergeAnimation(TileRuntime[] mergedTiles, float duration)
        {
            if (mergedTiles == null || mergedTiles.Length != 3) return;

            Vector3 centerPos = mergedTiles[1].transform.position;
            centerPos.z -= 1f;

            List<UniTask> animationTasks = new List<UniTask>();

            // 1. Gom vào giữa và hơi tụt xuống lấy đà (Anticipation)
            foreach (var tile in mergedTiles)
            {
                var moveTask = LMotion.Create(tile.transform.position, centerPos + Vector3.down * 0.2f, duration * 0.4f)
                    .WithEase(Ease.InOutSine)
                    .BindToPosition(tile.transform)
                    .ToUniTask();
                animationTasks.Add(moveTask);
            }

            await UniTask.WhenAll(animationTasks);
            animationTasks.Clear();

            // Play VFX lúc chạm nhau
            if (mergeVfxPrefab != null)
            {
                GameObject vfx = Object.Instantiate(mergeVfxPrefab, centerPos, Quaternion.identity);
                Object.Destroy(vfx, 2f);
            }

            // 2. Bay vút lên trên và thu nhỏ dần
            foreach (var tile in mergedTiles)
            {
                var flyTask = LMotion.Create(tile.transform.position, centerPos + Vector3.up * flyUpDistance,
                        duration * 0.6f)
                    .WithEase(Ease.OutBack)
                    .BindToPosition(tile.transform)
                    .ToUniTask();
                animationTasks.Add(flyTask);

                var scaleTask = LMotion.Create(tile.transform.localScale, Vector3.zero, duration * 0.6f)
                    .WithEase(Ease.InExpo)
                    .BindToLocalScale(tile.transform)
                    .ToUniTask();
                animationTasks.Add(scaleTask);
            }

            await UniTask.WhenAll(animationTasks);

            // Dọn dẹp Data
            foreach (var tile in mergedTiles)
            {
                tile.transform.localScale = Vector3.one;
                tile.transform.localRotation = Quaternion.identity;

                if (tile.Pool != null) tile.Pool.Release(tile);
                else Object.Destroy(tile.gameObject);
            }
        }
    }
}