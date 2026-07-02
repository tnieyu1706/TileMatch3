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
    public interface IBoardShuffleAnimStrategy
    {
        UniTask PlayShuffleAnimation(List<TileRuntime> tiles, Action doShuffleLogic);
    }
    
    [Serializable]
    public class ShrinkAndGrowShuffleStrategy : IBoardShuffleAnimStrategy
    {
        public float shrinkDuration = 0.2f;
        public float growDuration = 0.25f;
        [Tooltip("Thời gian chờ (ẩn) trước khi phóng to lại")]
        public float hideDelay = 0.1f;
        
        public Ease shrinkEase = Ease.InBack;
        public Ease growEase = Ease.OutBack;

        [Tooltip("Danh sách các Effect phát ra lúc bắt đầu Shuffle")] 
        [SerializeReference]
        public List<IEffectCommand> onStartEffects = new List<IEffectCommand>();

        public async UniTask PlayShuffleAnimation(List<TileRuntime> tiles, Action doShuffleLogic)
        {
            if (tiles == null || tiles.Count == 0)
            {
                doShuffleLogic?.Invoke();
                return;
            }

            // Chạy Effect (ví dụ: tiếng sfx "xào bài", vfx ánh sáng chớp lên)
            if (onStartEffects != null)
            {
                Vector3 centerPos = Vector3.zero;
                foreach (var cmd in onStartEffects)
                {
                    cmd.Execute(centerPos, shrinkDuration, Color.white);
                }
            }

            Vector3 originalScale = tiles[0].transform.localScale;

            // 1. Thu nhỏ toàn bộ các Tile về 0
            List<UniTask> shrinkTasks = new List<UniTask>();
            foreach (var tile in tiles)
            {
                var task = LMotion.Create(originalScale, Vector3.zero, shrinkDuration)
                    .WithEase(shrinkEase)
                    .BindToLocalScale(tile.transform)
                    .ToUniTask();
                shrinkTasks.Add(task);
            }
            await UniTask.WhenAll(shrinkTasks);

            // 2. Nghỉ một nhịp nhỏ (Tạo cảm giác đang xáo trộn)
            await UniTask.Delay(TimeSpan.FromSeconds(hideDelay));

            // 3. KÍCH HOẠT LOGIC TRÁO DATA
            // Toàn bộ Data và mảng màu sẽ thay đổi TỨC THÌ lúc này, nhưng vì size = 0 nên người chơi không thấy
            doShuffleLogic?.Invoke();

            // 4. Phóng to các Tile trở lại với "lớp áo" (Data) mới
            List<UniTask> growTasks = new List<UniTask>();
            foreach (var tile in tiles)
            {
                var task = LMotion.Create(Vector3.zero, originalScale, growDuration)
                    .WithEase(growEase)
                    .BindToLocalScale(tile.transform)
                    .ToUniTask();
                growTasks.Add(task);
            }
            await UniTask.WhenAll(growTasks);
        }
    }
}