using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace TileMatch3.Gameplay.HomeScene
{
    public class HomeSceneController : MonoBehaviour
    {
        [SerializeField] private RectTransform _logo;
        [SerializeField] private Button _playButton;
        
        private float defaultPlayBtnScale;

        private void Awake()
        {
            defaultPlayBtnScale = _playButton.transform.localScale.x;
        }

        public void OnPlayClicked()
        {
            Debug.Log("[HomeScene] Play clicked — navigation TBD");
        }

        public void OnPointerDown()
        {
            LMotion.Create(defaultPlayBtnScale, defaultPlayBtnScale * 0.92f, 0.1f)
                .WithEase(Ease.InBack)
                .BindToLocalScaleXYZ(_playButton.transform);
        }

        public void OnPointerUp()
        {
            LMotion.Create(defaultPlayBtnScale * 0.92f, defaultPlayBtnScale, 0.15f)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleXYZ(_playButton.transform);
        }
    }
}