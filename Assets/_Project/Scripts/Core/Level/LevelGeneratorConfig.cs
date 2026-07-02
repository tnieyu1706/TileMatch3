using TileMatch3.Core.Shape;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.Level
{
    /// <summary>
    /// Đóng vai trò làm Database cấu hình để AI bốc ngẫu nhiên
    /// </summary>
    [CreateAssetMenu(fileName = "LevelGeneratorConfig", menuName = "TileMatch3/Level Generator Config")]
    public class LevelGeneratorConfig : ScriptableObject
    {
        [Tooltip("Tất cả các hình dạng (Shape) có sẵn để thuật toán random chọn")]
        public TileShape[] allAvailableShapes;
        
        [Tooltip("Tất cả các loại TileData (Hình ảnh con vật/kẹo...) có trong game")]
        public TileData[] allAvailableTileTypes;
        
        public Vector2 defaultTileSize = new Vector2(1f, 1f);
    }
}