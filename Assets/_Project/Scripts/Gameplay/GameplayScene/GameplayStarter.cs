using Reflex.Attributes;
using TileMatch3.Core.Global;
using TileMatch3.Core.BoardSystem;
using TileMatch3.Core.Level;
using UnityEngine;

namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplayStarter : MonoBehaviour
    {
        [Header("System References")] [Inject] private BoardController boardController;

        [Inject] private RackController rackController;

        [Header("Level Configuration")] [Inject]
        private LevelGeneratorConfig levelConfig;

        [Inject] private GlobalGameplayDataVariable dataVariable;

        [Header("Rack Settings")] [SerializeField]
        private int rackSlotNumber = 7;

        private void Awake()
        {
            // TODO: register onWin, onLose handler for gameplay events
            dataVariable.Value.onPlayGame.AddListener(StartGame);
        }

        private void OnDestroy()
        {
            // TODO: un-register onWin, onLose handler for gameplay events
            dataVariable.Value.onPlayGame.RemoveListener(StartGame);
        }

        private void Start()
        {
            StartGame(dataVariable.Value.level);
        }

        public void StartGame(int levelIndex)
        {
            if (levelConfig == null || boardController == null || rackController == null)
            {
                Debug.LogError(
                    "[GameplayStarter] Thiếu Reference! Hãy kéo thả đủ BoardController, RackController và LevelGeneratorConfig vào Inspector.");
                return;
            }

            Debug.Log($"[GameplayStarter] Đang khởi tạo Level {levelIndex}...");

            // Sử dụng defaultTileSize.x từ config để quy định chiều ngang cho các khe trên Rack
            rackController.Setup(rackSlotNumber, levelConfig.defaultTileSize.x);

            LevelData levelData = LevelDataFactory.GenerateLevel(levelIndex, levelConfig);

            boardController.GenerateBoard(levelData, levelConfig);

            Debug.Log($"[GameplayStarter] Khởi tạo thành công!");
        }
    }
}