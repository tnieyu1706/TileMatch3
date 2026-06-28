using System.Collections.Generic;
using UnityEngine;
using TileMatch3.Core.Shape;

namespace TileMatch3.Core.Level
{
    public static class LevelDataFactory
    {
        public static LevelData GenerateLevel(int levelIndex, LevelGeneratorConfig config)
        {
            LevelData data = new LevelData();
            data.levelIndex = levelIndex;

            // 1. Tính toán số lượng Tile mục tiêu (Bắt đầu 20, tăng ~8% mỗi level)
            int rawTileCount = Mathf.RoundToInt(20 * Mathf.Pow(1.08f, levelIndex - 1));
            
            // Đảm bảo số lượng Tile LUÔN chia hết cho 3 (cơ chế cơ bản của Match 3)
            data.totalTileCount = rawTileCount - (rawTileCount % 3);
            if (data.totalTileCount < 3) data.totalTileCount = 3;

            // 2. Chọn ngẫu nhiên các Shape đắp vào các Layer cho đến khi sức chứa đủ số totalTileCount
            List<TileShape> selectedShapes = new List<TileShape>();
            int currentCapacity = 0;
            
            // Lặp đến khi tổng sức chứa của các layer >= tổng số tile cần thiết
            while (currentCapacity < data.totalTileCount)
            {
                TileShape randomShape = config.allAvailableShapes[Random.Range(0, config.allAvailableShapes.Length)];
                selectedShapes.Add(randomShape);
                
                currentCapacity += randomShape.GetActiveTileCount();
            }

            data.layerShapes = selectedShapes.ToArray();
            return data;
        }
    }
}