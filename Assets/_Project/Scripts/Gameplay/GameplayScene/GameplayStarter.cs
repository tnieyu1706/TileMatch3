using UnityEngine;
using TileMatch3.Core.BoardSystem;
using TileMatch3.Core.Level;

namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplayStarter : MonoBehaviour
    {
        [Header("System References")] [SerializeField]
        private BoardController boardController;

        [SerializeField] private RackController rackController;

        [Header("Level Configuration")] [SerializeField]
        private LevelGeneratorConfig levelConfig;

        [SerializeField] private int startLevel = 1;

        [Header("Rack Settings")] [SerializeField]
        private int rackSlotNumber = 7;

        private void Start()
        {
            StartGame(startLevel);
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