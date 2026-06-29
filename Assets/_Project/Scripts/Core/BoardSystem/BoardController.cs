using System;
using System.Collections.Generic;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using TileMatch3.Core.Global;
using TileMatch3.Core.Level;
using TileMatch3.Core.Tile;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace TileMatch3.Core.BoardSystem
{
    public class BoardController : MonoBehaviour
    {
        [Header("References")] [Inject] private RackController rackController;

        [Header("Board Settings")] [Inject] private GlobalGameplayDataVariable dataVariable;

        [SerializeField] private TileRuntime tilePrefab;
        [SerializeField] private Transform boardRoot;

        private LevelGeneratorConfig currentConfig;
        private readonly List<TileRuntime> activeTilesOnBoard = new List<TileRuntime>();

        private ObjectPool<TileRuntime> tilePool;

        [Header("View")] [SerializeField, ReadOnly]
        private LevelData currentLevelData;

        public event Action<List<TileRuntime>> OnBoardGenerated;
        public event Action<List<TileRuntime>, bool> OnBoardShuffling;

        private void Awake()
        {
            tilePool = new ObjectPool<TileRuntime>(
                createFunc: () => Instantiate(tilePrefab, boardRoot),
                actionOnGet: (tile) =>
                {
                    tile.gameObject.SetActive(true);
                    tile.ResetTile();
                },
                actionOnRelease: (tile) => tile.gameObject.SetActive(false),
                actionOnDestroy: (tile) => Destroy(tile.gameObject),
                defaultCapacity: 100,
                maxSize: 300
            );
        }

        public void GenerateBoard(LevelData levelData, LevelGeneratorConfig config)
        {
            currentLevelData = levelData;
            currentConfig = config;

            ClearBoard();

            // 1. Tính toán toàn bộ các vị trí hợp lệ trên Board (Có zic-zac offset)
            List<Vector3> boardPositions = GetBoardTilePositions(levelData);

            // 2. Spawn các cục Tile rỗng ra bàn dựa theo position
            SpawnEmptyTiles(boardPositions);
            
            // 3. Chuẩn bị Data (Từng bộ 3) và gán vào các Tile rỗng trên bàn
            AssignTileDataAndShuffle();

            // 4. Cập nhật trạng thái che khuất
            RefreshBoardState();
            OnBoardGenerated?.Invoke(activeTilesOnBoard);
        }

        /// <summary>
        /// Trả về danh sách vị trí chính xác của từng Tile. 
        /// Số lượng phần tử trả về sẽ MẶC ĐỊNH BẰNG ĐÚNG levelData.totalTileCount.
        /// </summary>
        private List<Vector3> GetBoardTilePositions(LevelData levelData)
        {
            List<Vector3> positions = new List<Vector3>();
            int tilesNeeded = levelData.totalTileCount;

            for (int layer = 0; layer < levelData.layerShapes.Length; layer++)
            {
                var shape = levelData.layerShapes[layer];

                // Random lệch x, y cho các layer cao (từ layer 1 trở lên) để tạo zic-zac
                float randomOffsetX = layer == 0 ? 0f : GetRandomOffset(currentConfig.defaultTileSize.x);
                float randomOffsetY = layer == 0 ? 0f : GetRandomOffset(currentConfig.defaultTileSize.y);

                float startX = -(shape.width * currentConfig.defaultTileSize.x) / 2f +
                               (currentConfig.defaultTileSize.x / 2f);
                float startY = (shape.height * currentConfig.defaultTileSize.y) / 2f -
                               (currentConfig.defaultTileSize.y / 2f);

                for (int y = 0; y < shape.height; y++)
                {
                    for (int x = 0; x < shape.width; x++)
                    {
                        if (shape.GetCell(x, y))
                        {
                            if (positions.Count >= tilesNeeded)
                                return positions; // Đã gom đủ tọa độ cần thiết, dừng luôn.

                            float posX = startX + (x * currentConfig.defaultTileSize.x) + randomOffsetX;
                            float posY = startY - (y * currentConfig.defaultTileSize.y) + randomOffsetY;
                            float posZ = -layer;

                            positions.Add(new Vector3(posX, posY, posZ));
                        }
                    }
                }
            }

            return positions;
        }

        private float GetRandomOffset(float size)
        {
            // Trả về ngẫu nhiên khoảng [0.2 đến 0.5] hoặc [-0.5 đến -0.2]
            float offset = Random.Range(0.2f, 0.5f);
            float sign = Random.value > 0.5f ? 1f : -1f;
            return offset * sign * size;
        }

        private void SpawnEmptyTiles(List<Vector3> positions)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 pos = positions[i];
                TileRuntime newTile = tilePool.Get();

                newTile.transform.position = pos;
                newTile.transform.localScale =
                    new Vector3(currentConfig.defaultTileSize.x, currentConfig.defaultTileSize.y, 1f);

                int layer = Mathf.Abs(Mathf.RoundToInt(pos.z));
                newTile.name = $"Tile_L{layer}_{i}";
                newTile.Pool = tilePool;
                newTile.OnTileClicked += HandleTileClicked;

                activeTilesOnBoard.Add(newTile);
            }
        }

        private void AssignTileDataAndShuffle()
        {
            // 1. Tạo danh sách TileData theo từng cụm 3
            List<TileData> generatedDataList = new List<TileData>();
            int tripletCount = activeTilesOnBoard.Count / 3;

            for (int i = 0; i < tripletCount; i++)
            {
                TileData randomType =
                    currentConfig.allAvailableTileTypes[Random.Range(0, currentConfig.allAvailableTileTypes.Length)];
                generatedDataList.Add(randomType);
                generatedDataList.Add(randomType);
                generatedDataList.Add(randomType);
            }

            // 2. Gán Data tuần tự vào các Tile trên bàn
            for (int i = 0; i < activeTilesOnBoard.Count; i++)
            {
                int layer = Mathf.Abs(Mathf.RoundToInt(activeTilesOnBoard[i].transform.position.z));
                activeTilesOnBoard[i].SetData(generatedDataList[i], layer);
            }

            // 3. Đảo vị trí (Shuffle) dữ liệu của các Tile trên bàn
            ShuffleBoard();
        }

        /// <summary>
        /// Hàm này có thể được gọi lại sau này như một Power-Up trong game.
        /// Nó sẽ lấy toàn bộ Data của các ngói đang nằm trên bàn, xáo trộn, và gán lại.
        /// </summary>
        public void ShuffleBoard(bool playEffect = false)
        {
            OnBoardShuffling?.Invoke(activeTilesOnBoard, playEffect);

            // 1. Lấy danh sách các THAM CHIẾU (references) đến file ScriptableObject gốc đang nằm trên bàn
            List<TileData> extractedData = new List<TileData>();
            foreach (var tile in activeTilesOnBoard)
            {
                // Vì CurrentTileData là reference đến file Asset thật, ta chỉ cần nhét nó vào list
                extractedData.Add(tile.CurrentTileData);
            }

            // 2. Xáo trộn danh sách tham chiếu này
            ShuffleList(extractedData);

            // 3. Gán ngược lại Data cho các Tile trên bàn
            for (int i = 0; i < activeTilesOnBoard.Count; i++)
            {
                int layer = Mathf.Abs(Mathf.RoundToInt(activeTilesOnBoard[i].transform.position.z));
                activeTilesOnBoard[i].SetData(extractedData[i], layer);
            }

            // Xong! Không cần phải xoá temp data nữa vì chúng ta dùng thẳng hàng real.
        }

        private void HandleTileClicked(TileRuntime clickedTile)
        {
            if (rackController.IsRackFull()) return;

            clickedTile.OnTileClicked -= HandleTileClicked;
            activeTilesOnBoard.Remove(clickedTile);

            rackController.Push(clickedTile);
            RefreshBoardState();

            if (activeTilesOnBoard.Count == 0)
            {
                Debug.Log("Board Clear! You Win!");
                dataVariable?.Value.onGameWin?.Invoke();
            }
        }

        private void RefreshBoardState()
        {
            Vector2 tileSize = currentConfig.defaultTileSize;
            float epsilon = 0.05f;

            foreach (var bottomTile in activeTilesOnBoard)
            {
                bool isCovered = false;

                foreach (var topTile in activeTilesOnBoard)
                {
                    if (topTile.transform.position.z < bottomTile.transform.position.z - 0.01f)
                    {
                        float distX = Mathf.Abs(topTile.transform.position.x - bottomTile.transform.position.x);
                        float distY = Mathf.Abs(topTile.transform.position.y - bottomTile.transform.position.y);

                        if (distX < (tileSize.x - epsilon) && distY < (tileSize.y - epsilon))
                        {
                            isCovered = true;
                            break;
                        }
                    }
                }

                bottomTile.SetState(isCovered ? TileState.Hidden : TileState.Normal);
            }
        }

        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void ClearBoard()
        {
            foreach (var tile in activeTilesOnBoard)
            {
                if (tile != null) tilePool.Release(tile);
            }

            activeTilesOnBoard.Clear();
        }
    }
}