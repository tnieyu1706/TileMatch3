using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TileMatch3.Core.EffectSystem.Commands;
using TileMatch3.Core.Tile;
using UnityEngine;

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

        [Tooltip("Danh sách các Effect (VFX, SFX...) phát ra tại tâm khi merge thành công")] [SerializeReference]
        public List<IEffectCommand> mergeEffects = new List<IEffectCommand>();

        public async UniTask PlayMergeAnimation(TileRuntime[] mergedTiles, float duration)
        {
            if (mergedTiles == null || mergedTiles.Length != 3) return;

            var mainTile = mergedTiles[1];
            Vector3 centerPos = mainTile.transform.position;
            centerPos.z -= 1f;

            List<UniTask> animationTasks = new List<UniTask>();

            foreach (var tile in mergedTiles)
            {
                var moveTask = LMotion.Create(tile.transform.position, centerPos, duration)
                    .WithEase(Ease.InQuad)
                    .BindToPosition(tile.transform)
                    .ToUniTask();
                animationTasks.Add(moveTask);

                var scaleTask = LMotion.Create(tile.transform.localScale, Vector3.zero, duration)
                    .WithEase(mergeEase)
                    .BindToLocalScale(tile.transform)
                    .ToUniTask();
                animationTasks.Add(scaleTask);
            }

            await UniTask.WhenAll(animationTasks);

            Debug.Log("Play MergeToCenter Animation...");
            // Truyền trực tiếp vào Execute
            if (mergeEffects != null && mergeEffects.Count > 0)
            {
                foreach (var effectCmd in mergeEffects)
                {
                    effectCmd.Execute(centerPos, 1f, mainTile.MainColor);
                }
            }
        }
    }

    [Serializable]
    public class MergeAndFlyUpStrategy : IMergeAnimStrategy
    {
        [Tooltip("Độ cao bay lên trước khi biến mất")]
        public float flyUpDistance = 1.5f;

        [Tooltip("Danh sách các Effect phát ra lúc chạm nhau ở giữa")] [SerializeReference]
        public List<IEffectCommand> mergeEffects = new List<IEffectCommand>();

        public async UniTask PlayMergeAnimation(TileRuntime[] mergedTiles, float duration)
        {
            if (mergedTiles == null || mergedTiles.Length != 3) return;

            var mainTile = mergedTiles[1];
            Vector3 centerPos = mainTile.transform.position;
            centerPos.z -= 1f;

            List<UniTask> animationTasks = new List<UniTask>();

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

            // Truyền trực tiếp vào Execute
            if (mergeEffects != null)
            {
                foreach (var effectCmd in mergeEffects)
                {
                    effectCmd.Execute(centerPos, 1f, mainTile.CurrentTileData.mainColor);
                }
            }

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
        }
    }

    [Serializable]
    public class MergeDissolveStrategy : IMergeAnimStrategy
    {
        [Tooltip("Loại Ease khi dissolve/thu nhỏ tại chỗ")]
        public Ease dissolveEase = Ease.InBack;

        [Tooltip("Hiệu ứng gọi 1 LẦN DUY NHẤT (Thường dùng cho SFX, Shake...)")] [SerializeReference]
        public List<IEffectCommand> centerEffects = new List<IEffectCommand>();

        [Tooltip("Hiệu ứng gọi trên TỪNG TILE (Thường dùng cho VFX nổ từng cục)")] [SerializeReference]
        public List<IEffectCommand> perTileEffects = new List<IEffectCommand>();

        public async UniTask PlayMergeAnimation(TileRuntime[] mergedTiles, float duration)
        {
            if (mergedTiles == null || mergedTiles.Length == 0) return;

            List<UniTask> animationTasks = new List<UniTask>();

            Vector3 centerPos = Vector3.zero;
            foreach (var tile in mergedTiles)
            {
                centerPos += tile.transform.position;
            }

            centerPos /= mergedTiles.Length;

            // Truyền trực tiếp vào Execute
            if (centerEffects != null)
            {
                foreach (var cmd in centerEffects)
                {
                    cmd.Execute(centerPos, duration, mergedTiles[0].MainColor);
                }
            }

            foreach (var tile in mergedTiles)
            {
                var scaleTask = LMotion.Create(tile.transform.localScale, Vector3.zero, duration)
                    .WithEase(dissolveEase)
                    .BindToLocalScale(tile.transform)
                    .ToUniTask();
                animationTasks.Add(scaleTask);

                // Truyền trực tiếp vào Execute
                if (perTileEffects != null)
                {
                    Vector3 vfxPos = tile.transform.position;
                    vfxPos.z -= 1f;

                    foreach (var cmd in perTileEffects)
                    {
                        cmd.Execute(vfxPos, 1f, tile.CurrentTileData.mainColor);
                    }
                }
            }

            await UniTask.WhenAll(animationTasks);
        }
    }
}