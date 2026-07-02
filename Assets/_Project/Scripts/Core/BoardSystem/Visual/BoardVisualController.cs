using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using TileMatch3.Core.BoardSystem.Animations;
using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.BoardSystem
{
    public class BoardVisualController : MonoBehaviour
    {
        [Inject] private BoardController boardController;

        [Header("Animation Strategies")] [SerializeReference]
        private List<IBoardGenerateAnimStrategy> generateStrategies = new();

        [SerializeReference] private List<IBoardShuffleAnimStrategy> shuffleStrategies = new();

        private void OnEnable()
        {
            boardController.OnBoardGenerated += HandleBoardGenerated;
            boardController.OnBoardShufflingAnim += HandleBoardShuffling;
        }

        private void OnDisable()
        {
            boardController.OnBoardGenerated -= HandleBoardGenerated;
            boardController.OnBoardShufflingAnim -= HandleBoardShuffling;
        }

        private void HandleBoardGenerated(List<TileRuntime> activeTiles)
        {
            if (generateStrategies != null && generateStrategies.Count > 0)
            {
                // Chọn ngẫu nhiên 1 loại hiệu ứng xuất hiện ban đầu (nếu có nhiều)
                var randomStrategy = generateStrategies[UnityEngine.Random.Range(0, generateStrategies.Count)];
                randomStrategy.PlayGenerateAnimation(activeTiles).Forget();
            }
        }

        private async UniTask HandleBoardShuffling(List<TileRuntime> activeTiles, Action doShuffleLogic)
        {
            if (shuffleStrategies != null && shuffleStrategies.Count > 0)
            {
                // Chọn ngẫu nhiên 1 loại hiệu ứng tráo đổi (nếu có nhiều)
                var randomStrategy = shuffleStrategies[UnityEngine.Random.Range(0, shuffleStrategies.Count)];
                await randomStrategy.PlayShuffleAnimation(activeTiles, doShuffleLogic);
            }
            else
            {
                // Fallback: Nếu bạn chưa kéo config Strategy nào vào Inspector, nó sẽ chỉ tráo Data khô khan thôi.
                doShuffleLogic?.Invoke();
            }
        }
    }
}