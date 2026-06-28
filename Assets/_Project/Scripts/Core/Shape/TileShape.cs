using System.Linq;
using UnityEngine;

namespace TileMatch3.Core.Shape
{
    [CreateAssetMenu(fileName = "NewTileShape", menuName = "TileMatch3/Tile Shape")]
    public class TileShape : ScriptableObject
    {
        [Min(1)] public int width = 5;
        [Min(1)] public int height = 5;

        // Unity không serialize được mảng 2 chiều, nên ta dùng mảng 1 chiều và ẩn nó đi
        [HideInInspector]
        public bool[] gridData;
        
        public int GetActiveTileCount()
        {
            if (gridData == null) return 0;
            return gridData.Count(isActive => isActive);
        }

        /// <summary>
        /// Lấy giá trị tại tọa độ x, y
        /// </summary>
        public bool GetCell(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;
            return gridData[y * width + x];
        }

        /// <summary>
        /// Gán giá trị tại tọa độ x, y
        /// </summary>
        public void SetCell(int x, int y, bool value)
        {
            if (!IsValidCoordinate(x, y)) return;
            gridData[y * width + x] = value;
        }

        /// <summary>
        /// Clear toàn bộ shape về false
        /// </summary>
        public void Clear()
        {
            if (gridData == null) return;
            for (int i = 0; i < gridData.Length; i++)
            {
                gridData[i] = false;
            }
        }

        /// <summary>
        /// Thay đổi kích thước grid, cố gắng giữ lại dữ liệu cũ nếu có thể
        /// </summary>
        public void Resize(int newWidth, int newHeight)
        {
            if (newWidth <= 0) newWidth = 1;
            if (newHeight <= 0) newHeight = 1;

            bool[] newGrid = new bool[newWidth * newHeight];

            if (gridData != null)
            {
                // Copy dữ liệu cũ sang grid mới
                int minWidth = Mathf.Min(width, newWidth);
                int minHeight = Mathf.Min(height, newHeight);

                for (int y = 0; y < minHeight; y++)
                {
                    for (int x = 0; x < minWidth; x++)
                    {
                        newGrid[y * newWidth + x] = gridData[y * width + x];
                    }
                }
            }

            width = newWidth;
            height = newHeight;
            gridData = newGrid;
        }

        /// <summary>
        /// Đảm bảo gridData luôn được khởi tạo
        /// </summary>
        public void ValidateData()
        {
            if (gridData == null || gridData.Length != width * height)
            {
                Resize(width, height);
            }
        }

        private bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height && gridData != null;
        }
    }
}