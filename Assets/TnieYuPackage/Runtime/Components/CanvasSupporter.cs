using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace TnieYuPackage.Components
{
    [DefaultExecutionOrder(-15)]
    [RequireComponent(typeof(Canvas))]
    public class CanvasSupporter : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        void Awake()
        {
            canvas ??= GetComponent<Canvas>();
        }

        async void Start()
        {
            await Awaitable.NextFrameAsync();
            canvas.worldCamera = Registry<Camera>.GetFirst();
        }
    }
}