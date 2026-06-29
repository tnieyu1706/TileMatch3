using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using TileMatch3.Core.Global;
using TileMatch3.Core.Level;
using TileMatch3.Core.Tile;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

// Cần thêm để dùng UniTask trong Event

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

        public event Action OnPreBoardGenerating;
        
        // Thêm event báo hiệu Board đã tạo xong (truyền List Tile qua để diễn)
        public event Action<List<TileRuntime>> OnBoardGenerated;
        
        public event Action OnBoardShuffling;
        // Event truyền List Tile và 1 Action logic (chứa lệnh tráo data) sang cho Visual Controller
        public event Func<List<TileRuntime>, Action, UniTask> OnBoardShufflingAnim;

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

            OnPreBoardGenerating?.Invoke();

            ClearBoard();

            // 1. Tính toán toàn bộ các vị trí hợp lệ trên Board (Có zic-zac offset)
            List<Vector3> boardPositions = GetBoardTilePositions(levelData);

            // 2. Spawn các cục Tile rỗng ra bàn dựa theo position
            SpawnEmptyTiles(boardPositions);

            // 3. Chuẩn bị Data (Từng bộ 3) và gán vào các Tile rỗng trên bàn
            AssignTileDataAndShuffle();

            // 4. Cập nhật trạng thái che khuất
            RefreshBoardState();

            // 5. Bắn event cho Visual Controller diễn Animation Introduce
            OnBoardGenerated?.Invoke(activeTilesOnBoard);
        }

        private List<Vector3> GetBoardTilePositions(LevelData levelData)
        {
            List<Vector3> positions = new List<Vector3>();
            int tilesNeeded = levelData.totalTileCount;

            for (int layer = 0; layer < levelData.layerShapes.Length; layer++)
            {
                var shape = levelData.layerShapes[layer];

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
                                return positions; 

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

            for (int i = 0; i < activeTilesOnBoard.Count; i++)
            {
                int layer = Mathf.Abs(Mathf.RoundToInt(activeTilesOnBoard[i].transform.position.z));
                activeTilesOnBoard[i].SetData(generatedDataList[i], layer);
            }

            // Dùng hàm Shuffle ẩn (không bắn event Anim) để xáo trộn data lúc khởi tạo Board
            PerformShuffleLogic();
        }

        /// <summary>
        /// Tách logic đảo Data ra riêng để tái sử dụng
        /// </summary>
        private void PerformShuffleLogic()
        {
            List<TileData> extractedData = new List<TileData>();
            foreach (var tile in activeTilesOnBoard)
            {
                extractedData.Add(tile.CurrentTileData);
            }

            ShuffleList(extractedData);

            for (int i = 0; i < activeTilesOnBoard.Count; i++)
            {
                int layer = Mathf.Abs(Mathf.RoundToInt(activeTilesOnBoard[i].transform.position.z));
                activeTilesOnBoard[i].SetData(extractedData[i], layer);
            }
        }

        /// <summary>
        /// Hàm này gọi bằng Power-Up: Có bắn event Animation
        /// </summary>
        [Button]
        public async void ShuffleBoard()
        {
            OnBoardShuffling?.Invoke(); // Vẫn giữ cho các UI khác lắng nghe

            // Gói toàn bộ logic tráo data + refresh trạng thái vào 1 Action
            Action shuffleLogic = () => 
            {
                PerformShuffleLogic();
                RefreshBoardState();
            };

            if (OnBoardShufflingAnim != null)
            {
                // Truyền Action sang bên Visual để nó tự canh thời gian kích hoạt (lúc đã thu nhỏ xong)
                await OnBoardShufflingAnim.Invoke(activeTilesOnBoard, shuffleLogic);
            }
            else
            {
                // Fallback nếu không gắn visual
                shuffleLogic.Invoke();
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