using Reflex.Attributes;
using TileMatch3.Core.Global;
using TMPro;
using UnityEngine;

namespace TileMatch3.Gameplay.GameplayScene.UI
{
    public class HeaderGameplayTextBinding : MonoBehaviour
    {
        [SerializeField] private string prefix = "Level";
        [SerializeField] private TextMeshProUGUI headerText;
        [Inject] private GlobalGameplayDataVariable dataVariable;

        private void Awake()
        {
            headerText ??= GetComponent<TextMeshProUGUI>();

            if (dataVariable != null)
            {
                dataVariable.Value.onPlayGame.AddListener(OnLevelGameChanged);
            }
        }

        private void Start()
        {
            OnLevelGameChanged(dataVariable?.Value.level ?? 1);
        }

        private void OnLevelGameChanged(int newLevel)
        {
            headerText.text = $"{prefix} {newLevel}";
        }

        private void OnDestroy()
        {
            if (dataVariable != null)
            {
                dataVariable.Value.onPlayGame.RemoveListener(OnLevelGameChanged);
            }
        }
    }
}