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
    public interface IMoveAnimStrategy
    {
        UniTask PlayMoveAnimation(TileRuntime tile, Vector2 targetPos, float duration, float delay, bool isPlayEffect);
    }

    [Serializable]
    public class MoveAndWobbleStrategy : IMoveAnimStrategy
    {
        [Tooltip("Loại Ease khi bay (Khuyên dùng OutCubic hoặc OutQuart để bay nhanh - đáp chậm)")]
        public Ease moveEase = Ease.OutCubic;

        [Tooltip("Góc lắc lư (độ)")] public float wobbleAngle = 15f;

        [Tooltip("Thời gian lắc lư (tính theo giây)")]
        public float wobbleDuration = 0.15f;

        [Tooltip("Danh sách các Effect phát ra khi bắt đầu bay")] [SerializeReference]
        public List<IEffectCommand> onStartEffects = new List<IEffectCommand>();

        public async UniTask PlayMoveAnimation(TileRuntime tile, Vector2 targetPos, float duration, float delay,
            bool isPlayEffect)
        {
            // 0. Thực thi Effect ngay khi bắt đầu
            if (isPlayEffect && onStartEffects != null)
            {
                foreach (var cmd in onStartEffects)
                {
                    cmd.Execute(tile.transform.position, duration, tile.CurrentTileData.mainColor);
                }
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: tile.GetCancellationTokenOnDestroy());

            // 3. Hiệu ứng lắc lư (Wobble) - Xoay qua trái, phải rồi về vị trí cũ
            // Lắc qua một bên
            await LMotion.Create(0f, wobbleAngle, wobbleDuration / 3f)
                .WithEase(Ease.OutQuad)
                .Bind(x => tile.transform.localRotation = Quaternion.Euler(0, 0, x))
                .ToUniTask();

            // Lắc qua bên ngược lại
            await LMotion.Create(wobbleAngle, -wobbleAngle, wobbleDuration / 3f)
                .WithEase(Ease.InOutQuad)
                .Bind(x => tile.transform.localRotation = Quaternion.Euler(0, 0, x))
                .ToUniTask();

            // Trở về vị trí cân bằng
            await LMotion.Create(-wobbleAngle, 0f, wobbleDuration / 3f)
                .WithEase(Ease.InQuad)
                .Bind(x => tile.transform.localRotation = Quaternion.Euler(0, 0, x))
                .ToUniTask();

            // Đảm bảo góc xoay reset về 0 tuyệt đối để tránh sai số
            tile.transform.localRotation = Quaternion.identity;
        }
    }

    [Serializable]
    public class MoveAndBounceStrategy : IMoveAnimStrategy
    {
        [Tooltip("Loại Ease khi bay")] public Ease moveEase = Ease.OutQuart;

        [Tooltip("Độ nảy (trục Y)")] public float bounceHeight = 0.5f;

        [Tooltip("Thời gian nảy")] public float bounceDuration = 0.15f;

        [Tooltip("Danh sách các Effect phát ra khi bắt đầu bay")] [SerializeReference]
        public List<IEffectCommand> onStartEffects = new List<IEffectCommand>();

        public async UniTask PlayMoveAnimation(TileRuntime tile, Vector2 targetPos, float duration, float delay,
            bool isPlayEffect)
        {
            // 0. Thực thi Effect ngay khi bắt đầu
            if (isPlayEffect && onStartEffects != null)
            {
                foreach (var cmd in onStartEffects)
                {
                    cmd.Execute(tile.transform.position, duration, tile.CurrentTileData.mainColor);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: tile.GetCancellationTokenOnDestroy());

            // Hiệu ứng nảy lên (Bounce Y)
            float startY = targetPos.y;

            // Nảy lên
            await LMotion.Create(startY, startY + bounceHeight, bounceDuration / 2f)
                .WithEase(Ease.OutQuad)
                .Bind(y => tile.transform.position =
                    new Vector3(tile.transform.position.x, y, tile.transform.position.z))
                .ToUniTask();

            // Rơi xuống
            await LMotion.Create(startY + bounceHeight, startY, bounceDuration / 2f)
                .WithEase(Ease.InQuad)
                .Bind(y => tile.transform.position =
                    new Vector3(tile.transform.position.x, y, tile.transform.position.z))
                .ToUniTask();
        }
    }
}