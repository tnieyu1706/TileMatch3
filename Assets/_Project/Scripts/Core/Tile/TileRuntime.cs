using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace TileMatch3.Core.Tile
{
    public enum TileState
    {
        Normal,
        Hidden
    }

    public class TileRuntime : MonoBehaviour, IPointerDownHandler
    {
        public bool isOnRack;
        public TileState CurrentState { get; private set; }

        // Lưu giữ tham chiếu đến Data gốc để xài cho hàm ShuffleBoard
        [field:SerializeField]
        public TileData CurrentTileData { get; private set; }
        public Guid TileId => CurrentTileData.id;
        public Color MainColor => CurrentTileData.mainColor;

        [SerializeField] private SpriteRenderer baseTileRenderer;
        [SerializeField] private SpriteRenderer iconIdRenderer;

        public event Action<TileRuntime> OnTileClicked;

        // Reference tới Pool để có thể tự release chính mình
        public IObjectPool<TileRuntime> Pool { get; set; }

        public void ResetTile()
        {
            isOnRack = false;
            OnTileClicked = null;
            CurrentTileData = null;
        }

        public void SetData(TileData tileData, int layer)
        {
            CurrentTileData = tileData; // Lưu lại để Shuffle
            iconIdRenderer.sprite = tileData.tileSprite;

            // Xử lý Render Order: Nền = layer * 2, Icon = layer * 2 + 1 để không bị đè xuyên
            SetSortingOrder(layer * 2);
        }

        // Helper cho hàm Shuffle
        public Sprite GetSprite() => iconIdRenderer.sprite;

        public void SetState(TileState state)
        {
            CurrentState = state;

            baseTileRenderer.color = iconIdRenderer.color =
                state == TileState.Hidden ? Color.gray : Color.white;
        }

        public void SetSortingOrder(int order)
        {
            baseTileRenderer.sortingOrder = order + 1;
            iconIdRenderer.sortingOrder = order + 2;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isOnRack) return;

            if (CurrentState == TileState.Normal)
            {
                OnTileClicked?.Invoke(this);
            }
        }
    }
}