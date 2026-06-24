using UnityEngine;

namespace TnieYuPackage.Components
{
    [ExecuteAlways]
    public class ObjectColor : MonoBehaviour
    {
        [SerializeField] private Color _color = Color.white;

        private MaterialPropertyBlock _mpb;
        private Renderer _renderer;

        void OnValidate()
        {
            Apply();
        }

        void Awake() => Apply();

        void Apply()
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();
            if (_mpb == null) _mpb = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_BaseColor", _color);
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}