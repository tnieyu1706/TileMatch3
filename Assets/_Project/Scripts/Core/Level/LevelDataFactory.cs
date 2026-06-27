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

            // Tính toán target tile: Bắt đầu khoảng 20, tăng ~8% mỗi level
            int targetTileCount = Mathf.RoundToInt(20 * Mathf.Pow(1.08f, levelIndex - 1));
            
            // Tính số layer tối thiểu (Level càng cao layer càng sâu, min là 2)
            int targetLayers = Mathf.Clamp(2 + (levelIndex / 4), 2, 7);

            List<TileShape> selectedShapes = new List<TileShape>();
            int currentActiveCells = 0;
            
            // AI chọn Shape ngẫu nhiên đắp vào các layer cho đến khi đạt target
            while (currentActiveCells < targetTileCount || selectedShapes.Count < targetLayers)
            {
                TileShape randomShape = config.allAvailableShapes[Random.Range(0, config.allAvailableShapes.Length)];
                selectedShapes.Add(randomShape);
                
                foreach (bool isActive in randomShape.gridData)
                {
                    if (isActive) currentActiveCells++;
                }
            }

            data.layerShapes = selectedShapes.ToArray();
            return data;
        }
    }
}