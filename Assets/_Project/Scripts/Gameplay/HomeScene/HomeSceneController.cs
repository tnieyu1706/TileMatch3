using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TileMatch3.Gameplay.HomeScene
{
    public class HomeSceneController : MonoBehaviour
    {
        [SerializeField] private RectTransform _logo;
        [SerializeField] private Button _playButton;

        private IDisposable _pulseMotion;

        private void Start()
        {
            AnimateLogoEntrance();
            AnimateButtonPulse();
        }

        private void AnimateLogoEntrance()
        {
            _logo.localScale = Vector3.one * 0.8f;
            _logo.GetComponent<CanvasGroup>().alpha = 0f;

            LMotion.Create(0.8f, 1f, 0.5f)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleX(_logo);

            LMotion.Create(0.8f, 1f, 0.5f)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleY(_logo);

            LMotion.Create(0f, 1f, 0.3f)
                .WithDelay(0.1f)
                .BindToAlpha(_logo.GetComponent<CanvasGroup>());
        }

        private void AnimateButtonPulse()
        {
            var seq = LSequence.Create()
                .Append(LMotion.Create(1f, 1.05f, 1f)
                    .WithEase(Ease.InOutSine))
                .Append(LMotion.Create(1.05f, 1f, 1f)
                    .WithEase(Ease.InOutSine));

            _pulseMotion = seq.Run()
                .Preserve()
                .AddTo(gameObject);
        }

        public void OnPlayClicked()
        {
            Debug.Log("[HomeScene] Play clicked — navigation TBD");
        }

        public void OnPointerDown()
        {
            LMotion.Create(1f, 0.92f, 0.1f)
                .WithEase(Ease.InBack)
                .BindToLocalScale(_playButton.transform);
        }

        public void OnPointerUp()
        {
            LMotion.Create(0.92f, 1f, 0.15f)
                .WithEase(Ease.OutBack)
                .BindToLocalScale(_playButton.transform);
        }

        private void OnDestroy()
        {
            _pulseMotion?.Dispose();
        }
    }
}
