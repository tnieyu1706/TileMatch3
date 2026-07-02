using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileMatch3.Core.Combo
{
    public class ComboDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform displayArea;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private Image comboBlurBackground;
        [Header("Font Material Setup")]
        [SerializeField] private float outlineWidth = 0.25f;

        [SerializeField] private float faceSoftness = 0.5f;
        [SerializeField] private float faceDilate = 0.3f;

        [Header("Combo Settings")]
        [SerializeField] private Color lerpColor = Color.white;
        [SerializeField] private float lerpRatio = 0.3f;

        private Material fontInstanceMaterial;

        void Awake()
        {
            fontInstanceMaterial = comboText.fontMaterial;
        }

        void Start()
        {
            SetupFontMaterial(fontInstanceMaterial);
            displayArea.gameObject.SetActive(false);
        }

        private void SetupFontMaterial(Material fontMaterial)
        {
            fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
            fontMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, faceSoftness);
            fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
        }

        public async UniTaskVoid DisplayCombo(ComboInfo comboInfo, float duration)
        {
            comboBlurBackground.color = comboInfo.color;
            comboText.text = comboInfo.text;
            fontInstanceMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.Lerp(comboInfo.color, lerpColor, lerpRatio));
            
            displayArea.gameObject.SetActive(true);
            comboInfo.sfxCommand.Execute(displayArea.transform.position, duration, null);
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: this.GetCancellationTokenOnDestroy());
            displayArea.gameObject.SetActive(false);
        }
    }
}