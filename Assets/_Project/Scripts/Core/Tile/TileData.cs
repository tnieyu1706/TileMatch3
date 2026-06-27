using System;
using TnieYuPackage.Utils;
using UnityEngine;

namespace TileMatch3.Core.Tile
{
    public class TileData : ScriptableObject
    {
        public SerializableGuid id = Guid.NewGuid();
        public Sprite tileSprite;
    }
}