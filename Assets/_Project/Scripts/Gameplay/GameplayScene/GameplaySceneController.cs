using UnityEngine;
using UnityEngine.UI;

namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplaySceneController : MonoBehaviour
    {
        [SerializeField] private Button _btnLeftAction;
        [SerializeField] private Button _btnRightAction;
        [SerializeField] private Button[] _boosterSlots;

        private void Awake()
        {
            if (_btnLeftAction != null)
                _btnLeftAction.onClick.AddListener(OnLeftActionClicked);
            if (_btnRightAction != null)
                _btnRightAction.onClick.AddListener(OnRightActionClicked);
            for (int i = 0; i < _boosterSlots.Length; i++)
            {
                var index = i;
                if (_boosterSlots[i] != null)
                    _boosterSlots[i].onClick.AddListener(() => OnBoosterClicked(index));
            }
        }

        public void OnLeftActionClicked()
        {
            Debug.Log("[GameplayScene] Left action clicked");
        }

        public void OnRightActionClicked()
        {
            Debug.Log("[GameplayScene] Right action clicked");
        }

        public void OnBoosterClicked(int index)
        {
            Debug.Log($"[GameplayScene] Booster {index} clicked");
        }
    }
}
