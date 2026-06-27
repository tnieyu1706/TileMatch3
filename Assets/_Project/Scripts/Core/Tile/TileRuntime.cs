using System;
using KBCore.Refs;
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

        [SerializeField, Child] private SpriteRenderer iconIdRenderer;
        [SerializeField] private SpriteRenderer backgroundRenderer; 

        public event Action<TileRuntime> OnTileClicked;
        
        // Reference tới Pool để có thể tự release chính mình
        public IObjectPool<TileRuntime> Pool { get; set; }

        public void ResetTile()
        {
            isOnRack = false;
            OnTileClicked = null;
            // Scale sẽ được BoardController ghi đè lại sau
            SetSortingOrder(0);
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
            
            if (backgroundRenderer != null)
                backgroundRenderer.color = state == TileState.Hidden ? Color.gray : Color.white;
            if (iconIdRenderer != null)
                iconIdRenderer.color = state == TileState.Hidden ? Color.gray : Color.white;
        }

        public void SetSortingOrder(int order)
        {
            if (iconIdRenderer != null) iconIdRenderer.sortingOrder = order + 1;
            
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = order;
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