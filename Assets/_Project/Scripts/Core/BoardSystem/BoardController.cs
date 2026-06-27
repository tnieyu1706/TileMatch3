using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using TileMatch3.Core.Level;
using TileMatch3.Core.Tile;

namespace TileMatch3.Core.BoardSystem
{
    public class BoardController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RackController rackController;
        [SerializeField] private TileRuntime tilePrefab;
        [SerializeField] private Transform boardRoot;

        private List<TileRuntime> activeTilesOnBoard = new List<TileRuntime>();
        private LevelData currentLevelData;
        private LevelGeneratorConfig currentConfig; // Tham chiếu đến config pool
        
        private ObjectPool<TileRuntime> tilePool;

        private void Awake()
        {
            tilePool = new ObjectPool<TileRuntime>(
                createFunc: () => Instantiate(tilePrefab, boardRoot),
                actionOnGet: (tile) => { 
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

            int totalActiveCells = 0;
            foreach (var shape in levelData.layerShapes)
            {
                if (shape.gridData == null) continue;
                foreach (bool isActive in shape.gridData)
                {
                    if (isActive) totalActiveCells++;
                }
            }

            int remainder = totalActiveCells % 3;
            if (remainder != 0)
                totalActiveCells -= remainder;

            // 4. Cho phép bốc ngẫu nhiên từ TẤT CẢ các loại Tile có trong config
            List<TileData> generatedTileData = GenerateTilePairs(totalActiveCells, config.allAvailableTileTypes);
            ShuffleList(generatedTileData);
            SpawnTiles(levelData, generatedTileData);
            
            RefreshBoardState();
        }

        private List<TileData> GenerateTilePairs(int totalTiles, TileData[] availableTypes)
        {
            List<TileData> dataList = new List<TileData>();
            int tripletCount = totalTiles / 3;

            for (int i = 0; i < tripletCount; i++)
            {
                TileData randomType = availableTypes[Random.Range(0, availableTypes.Length)];
                dataList.Add(randomType);
                dataList.Add(randomType);
                dataList.Add(randomType);
            }
            return dataList;
        }

        private float GetRandomOffset(float size)
        {
            // Trả về ngẫu nhiên khoảng [0.2 đến 0.5] hoặc [-0.5 đến -0.2]
            float offset = Random.Range(0.2f, 0.5f);
            float sign = Random.value > 0.5f ? 1f : -1f;
            return offset * sign * size;
        }

        private void SpawnTiles(LevelData levelData, List<TileData> shuffledData)
        {
            int dataIndex = 0;

            for (int layer = 0; layer < levelData.layerShapes.Length; layer++)
            {
                var shape = levelData.layerShapes[layer];
                
                float randomOffsetX = layer == 0 ? 0f : GetRandomOffset(currentConfig.defaultTileSize.x);
                float randomOffsetY = layer == 0 ? 0f : GetRandomOffset(currentConfig.defaultTileSize.y);

                float startX = -(shape.width * currentConfig.defaultTileSize.x) / 2f + (currentConfig.defaultTileSize.x / 2f);
                float startY = (shape.height * currentConfig.defaultTileSize.y) / 2f - (currentConfig.defaultTileSize.y / 2f);

                for (int y = 0; y < shape.height; y++)
                {
                    for (int x = 0; x < shape.width; x++)
                    {
                        if (shape.GetCell(x, y))
                        {
                            if (dataIndex >= shuffledData.Count) return;

                            float posX = startX + (x * currentConfig.defaultTileSize.x) + randomOffsetX;
                            float posY = startY - (y * currentConfig.defaultTileSize.y) + randomOffsetY;
                            
                            // 2. Chuyển Z thành nguyên túc (1 layer = 1 đơn vị Z)
                            float posZ = -layer; 

                            Vector3 finalPos = new Vector3(posX, posY, posZ);

                            TileRuntime newTile = tilePool.Get();
                            newTile.transform.position = finalPos;
                            
                            // 1. Gán kích thước (Scale) cho object
                            newTile.transform.localScale = new Vector3(currentConfig.defaultTileSize.x, currentConfig.defaultTileSize.y, 1f);
                            
                            newTile.name = $"Tile_L{layer}_{x}_{y}";
                            newTile.Pool = tilePool; 
                            
                            // 2. Truyền layer vào SetData để set Render Order
                            newTile.SetData(shuffledData[dataIndex], layer);
                            
                            newTile.OnTileClicked += HandleTileClicked;

                            activeTilesOnBoard.Add(newTile);
                            dataIndex++;
                        }
                    }
                }
            }
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