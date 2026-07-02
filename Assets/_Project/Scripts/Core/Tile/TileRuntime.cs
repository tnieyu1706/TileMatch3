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

        [SerializeField] private SpriteRenderer baseTileRenderer;
        [SerializeField] private SpriteRenderer iconIdRenderer;

        // Lưu giữ tham chiếu đến Data gốc để xài cho hàm ShuffleBoard
        [Header("Read-Only")]
        [field: SerializeField]
        public TileState CurrentState { get; private set; }

        [field: SerializeField] public TileData CurrentTileData { get; private set; }
        public Guid TileId => CurrentTileData.id;
        public Color MainColor => CurrentTileData.mainColor;

        public bool IsActive => CurrentState == TileState.Normal && gameObject.activeSelf;

        // Đã thay đổi: Thêm tham số bool để báo hiệu có cần check Rack Full không
        public event Action<TileRuntime, bool> OnTileClicked;
        public event Action<TileRuntime> OnTileNotAllowClicked;

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

            SetSortingOrder(layer);
        }

        public void SetState(TileState state)
        {
            CurrentState = state;

            baseTileRenderer.color = iconIdRenderer.color =
                state == TileState.Hidden ? Color.gray : Color.white;
        }

        public void SetSortingOrder(int order)
        {
            // Xử lý Render Order: Nền = order * 2, Icon = order * 2 + 1 để không bị đè xuyên
            var layer = order * 2;
            baseTileRenderer.sortingOrder = layer + 1;
            iconIdRenderer.sortingOrder = layer + 2;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isOnRack) return;

            switch (CurrentState)
            {
                case TileState.Normal:
                    // Click bình thường thì auto check full rack
                    OnTileClicked?.Invoke(this, true);
                    break;
                case TileState.Hidden:
                    // Bắn event báo hiệu Tile đang bị ẩn mà người chơi vẫn cố click
                    OnTileNotAllowClicked?.Invoke(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SimulateClick(bool checkRackFull)
        {
            if (isOnRack) return;
            
            OnTileClicked?.Invoke(this, checkRackFull);
        }
    }
}