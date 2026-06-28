using Reflex.Attributes;
using TileMatch3.Core.Global;
using UnityEngine;

namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplayMenuController : MonoBehaviour
    {
        [Header("Global Gameplay Settings")] [Inject]
        private GlobalGameplayDataVariable dataVariable;

        [Header("Gameplay Panel Settings")] [SerializeField]
        private CanvasGroup winPanel;

        [SerializeField] private CanvasGroup losePanel;

        private void Awake()
        {
            // TODO: register onWin, onLose ui handler (display panel) for gameplay events
            dataVariable.Value.onGameWin.AddListener(OpenWinPanel);
            dataVariable.Value.onGameLose.AddListener(OpenLosePanel);
        }

        private void Start()
        {
            CloseAllPanels();
        }

        private void OpenWinPanel()
        {
            winPanel.alpha = 1;
            winPanel.interactable = true;
            winPanel.blocksRaycasts = true;
        }

        private void OpenLosePanel()
        {
            losePanel.alpha = 1;
            losePanel.interactable = true;
            losePanel.blocksRaycasts = true;
        }

        private void OnDestroy()
        {
            // TODO: un-register onWin, onLose ui handler (display panel) for gameplay events
            dataVariable.Value.onGameWin.RemoveListener(OpenWinPanel);
            dataVariable.Value.onGameLose.RemoveListener(OpenLosePanel);
        }

        private void CloseAllPanels()
        {
            winPanel.alpha = losePanel.alpha = 0;
            winPanel.interactable = losePanel.interactable = false;
            winPanel.blocksRaycasts = losePanel.blocksRaycasts = false;
        }

        public void HandleNextBtnClicked()
        {
            Debug.Log("[GameplayScene] Win panel button clicked");
            CloseAllPanels();
            
            dataVariable.Value.level++;
            dataVariable.Value.onPlayGame.Invoke(dataVariable.Value.level);
        }

        public void HandleResetBtnClicked()
        {
            Debug.Log("[GameplayScene] Lose panel button clicked");
            CloseAllPanels();
            
            dataVariable.Value.onPlayGame.Invoke(dataVariable.Value.level);
        }
    }
}