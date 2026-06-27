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
        public Guid TileId;
        public bool isOnRack;
        public TileState CurrentState { get; private set; }

        [SerializeField] private SpriteRenderer baseTileRenderer;
        [SerializeField] private SpriteRenderer iconIdRenderer;

        public event Action<TileRuntime> OnTileClicked;

        // Reference tới Pool để có thể tự release chính mình
        public IObjectPool<TileRuntime> Pool { get; set; }

        public void ResetTile()
        {
            isOnRack = false;
            OnTileClicked = null;
        }

        public void SetData(TileData tileData, int layer)
        {
            iconIdRenderer.sprite = tileData.tileSprite;
            TileId = tileData.id;

            // Xử lý Render Order: Nền = layer * 2, Icon = layer * 2 + 1 để không bị đè xuyên
            SetSortingOrder(layer * 2);
        }

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