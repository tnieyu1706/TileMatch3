using System;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TileMatch3.Core.PowerUp
{
    public interface IPower
    {
        bool IsEnoughEnable { set; }
        bool IsDoubleCheck { get; }
        event Action OnClick;
        
        void OnStartWaiting();
        void OnProcessWaiting(float progress);
        void OnStopWaiting();

        void SetPowerOff();
        void SetPowerOn();
    }
    
    [DefaultExecutionOrder(-5)]
    public abstract class BasePower : MonoBehaviour, IPower
    {
        [SerializeField] protected Button button;
        [SerializeField] protected Image icon;
        [SerializeField] protected Image coverImage;

        [Header("Options")] [SerializeField] private Color offColor = Color.white.With(a: 0.5f);
        [SerializeField] private Color onColor = Color.white;

        public bool IsEnoughEnable { protected get; set; } = true;
        public bool IsDoubleCheck { get; protected set; } = true;
        public event Action OnClick;

        protected virtual void OnEnable()
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        
        protected virtual void OnButtonClicked()
        {
            OnClick?.Invoke();
        }

        protected virtual void OnDisable()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }

        public void OnStartWaiting()
        {
            SetPowerOff();
            coverImage.gameObject.SetActive(true);
        }

        public void OnProcessWaiting(float progress)
        {
            coverImage.fillAmount = progress;
        }

        public void OnStopWaiting()
        {
            SetPowerOn();
            coverImage.gameObject.SetActive(false);
            coverImage.fillAmount = 0f;
        }

        public void SetPowerOff()
        {
            button.interactable = false;
            icon.color = offColor;
        }

        public void SetPowerOn()
        {
            if (!IsEnoughEnable || !IsDoubleCheck) return;

            button.interactable = true;
            icon.color = onColor;
        }
    }
}