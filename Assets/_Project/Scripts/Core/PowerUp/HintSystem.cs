using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using TileMatch3.Core.BoardSystem;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.PowerUp
{
    public class HintSystem : MonoBehaviour
    {
        [SerializeField] private float eachDelay = 0.15f;
        [Inject] private BoardController boardController;
        [Inject] private RackController rackController;

        [Button("Execute Hint")]
        public async UniTask ExecuteHint()
        {
            var activeRackTiles = rackController.GetActiveRackTiles();
            Guid targetTileId;
            int currentCountOnRack = 0;

            // 1. Tìm Tile mục tiêu trên Rack
            if (activeRackTiles.Count > 0)
            {
                var lastTile = activeRackTiles.Last();
                targetTileId = lastTile.TileId;
                
                // Đếm xem trên rack đang có sẵn bao nhiêu cục giống vậy để lấy cho vừa đủ
                currentCountOnRack = activeRackTiles.Count(t => t.TileId == targetTileId);
            }
            else
            {
                // Fallback: Nếu Rack trống, tìm bừa 1 Tile Normal trên Board
                var normalTiles = boardController.GetActiveTilesFilter(t => t.CurrentState == TileState.Normal);
                if (normalTiles.Count == 0) return; // Board trống hoặc không còn tile hợp lệ
                
                targetTileId = normalTiles[0].TileId;
            }

            // Tính số lượng Tile còn thiếu để tạo thành bộ 3
            int neededTiles = 3 - currentCountOnRack;
            if (neededTiles <= 0) return;

            // 2. Lấy danh sách tile trên board khớp với TileId cần tìm
            var matchingTiles = boardController.GetActiveTilesFilter(t => t.TileId == targetTileId);

            // 3. Order ưu tiên: Normal (0) xếp trước, Hidden (1) xếp sau. Và chỉ lấy đủ số lượng cần thiết
            var orderedTiles = matchingTiles
                .OrderBy(t => t.CurrentState == TileState.Normal ? 0 : 1)
                .Take(neededTiles)
                .ToList();

            // 4. Thực thi việc đưa các Tile này xuống Rack. Truyền false để bỏ qua giới hạn Rack Full synchronous
            foreach (var tile in orderedTiles)
            {
                tile.SimulateClick(false);
                await UniTask.Delay(TimeSpan.FromSeconds(eachDelay));
            }
        }
    }
}