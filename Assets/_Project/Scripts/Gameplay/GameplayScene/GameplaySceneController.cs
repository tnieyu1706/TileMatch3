using UnityEngine;

namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplaySceneController : MonoBehaviour
    {
        public void HandleHomeBtnClicked()
        {
            Debug.Log("[GameplayScene] Home clicked");
        }

        public void HandleSettingsBtnClicked()
        {
            Debug.Log("[GameplayScene] Settings clicked");
        }
    }
}