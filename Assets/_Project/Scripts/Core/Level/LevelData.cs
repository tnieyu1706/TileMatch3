using TileMatch3.Core.Shape;
using UnityEngine;

namespace TileMatch3.Core.Level
{
    [System.Serializable]
    public class LevelData
    {
        public int levelIndex;
        
        [Tooltip("Danh sách các shape tương ứng với từng layer. Index 0 là layer dưới cùng.")]
        public TileShape[] layerShapes;
    }
}