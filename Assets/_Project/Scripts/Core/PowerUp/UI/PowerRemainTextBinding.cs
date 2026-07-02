using System;
using Obvious.Soap;
using TMPro;
using UnityEngine;

namespace TileMatch3.Core.PowerUp.UI
{
    public class PowerRemainTextBinding : MonoBehaviour
    {
        [SerializeField] private Transform powerRemainingArea;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private IntVariable powerRemainingVariable;

        private void Awake()
        {
            text ??= GetComponent<TextMeshProUGUI>();

            if (powerRemainingVariable != null)
            {
                powerRemainingVariable.OnValueChanged += OnRemainValueChanged;
            }
        }

        private void Start()
        {
            OnRemainValueChanged(powerRemainingVariable?.Value ?? 1);
        }

        private void OnRemainValueChanged(int newValue)
        {
            text.text = newValue.ToString();
            
            var isFormat = newValue > 1; // >1 = true (display)| <=1 = false (hidden)
            if (powerRemainingArea.gameObject.activeSelf != isFormat)
            {
                powerRemainingArea.gameObject.SetActive(isFormat);
            }
        }

        private void OnDestroy()
        {
            if (powerRemainingVariable != null)
            {
                powerRemainingVariable.OnValueChanged -= OnRemainValueChanged;
            }
        }
    }
}