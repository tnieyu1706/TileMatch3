using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TileMatch3.Core.EffectSystem.Commands;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.BoardSystem.Animations
{
    public interface IBoardGenerateAnimStrategy
    {
        UniTask PlayGenerateAnimation(List<TileRuntime> tiles);
    }

    [Serializable]
    public class LayerByLayerGenerateStrategy : IBoardGenerateAnimStrategy
    {
        [Tooltip("Thời gian xuất hiện của từng layer")]
        public float durationPerLayer = 0.25f;
        public Ease scaleEase = Ease.OutBack;

        [Tooltip("Danh sách các Effect phát ra lúc bắt đầu Generate")] 
        [SerializeReference]
        public List<IEffectCommand> onStartEffects = new List<IEffectCommand>();

        public async UniTask PlayGenerateAnimation(List<TileRuntime> tiles)
        {
            if (tiles == null || tiles.Count == 0) return;

            // Chạy Effect đầu tiên (ví dụ: tiếng nhạc xuất hiện màn chơi)
            if (onStartEffects != null)
            {
                // Lấy vị trí trung tâm cơ bản
                Vector3 centerPos = Vector3.zero;
                foreach (var cmd in onStartEffects)
                {
                    cmd.Execute(centerPos, durationPerLayer, Color.white);
                }
            }

            // Lưu lại scale mặc định (size của ngói theo config)
            Vector3 targetScale = tiles[0].transform.localScale;

            // Phân nhóm các tile theo Layer (dựa vào z) và sort từ dưới cùng (z lớn nhất) lên trên cùng
            var layerGroups = tiles.GroupBy(t => Mathf.Abs(Mathf.RoundToInt(t.transform.position.z)))
                                   .OrderBy(g => g.Key)
                                   .ToList();

            // Khởi tạo: Đặt scale toàn bộ về 0 ngay lập tức
            foreach (var tile in tiles)
            {
                tile.transform.localScale = Vector3.zero;
            }

            // Diễn xuất hiện lần lượt từng Layer
            foreach (var group in layerGroups)
            {
                List<UniTask> tasks = new List<UniTask>();
                foreach (var tile in group)
                {
                    var task = LMotion.Create(Vector3.zero, targetScale, durationPerLayer)
                        .WithEase(scaleEase)
                        .BindToLocalScale(tile.transform)
                        .ToUniTask();
                    tasks.Add(task);
                }
                
                // Đợi layer hiện tại bung xong mới cho layer tiếp theo bung
                await UniTask.WhenAll(tasks);
            }
        }
    }
}