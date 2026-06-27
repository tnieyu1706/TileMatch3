using TileMatch3.Core.Tile;
using UnityEngine;

namespace TileMatch3.Core.BoardSystem
{
    public class RackItem : MonoBehaviour
    {
        public TileRuntime itemTile;

        public async Awaitable SetItemTile(TileRuntime tile, float delay)
        {
            itemTile = tile;

            gameObject.SetActive(true);
            await Awaitable.WaitForSecondsAsync(delay);

            itemTile.transform.SetParent(transform);
        }

        public async Awaitable RemoveItemTile(float delay)
        {
            itemTile.transform.SetParent(null);
            itemTile = null;

            await Awaitable.WaitForSecondsAsync(delay);
            gameObject.SetActive(false);
        }
    }
}