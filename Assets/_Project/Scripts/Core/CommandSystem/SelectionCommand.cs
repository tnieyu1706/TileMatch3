using System;
using UnityEngine;

namespace TileMatch3.Core.CommandSystem
{
    public class SelectionCommand : ICommand
    {
        public readonly int RackIndex;
        public readonly (int layer, Vector2 boardPos) TilePos;
        public readonly Guid TileId;

        public SelectionCommand(Guid tileId, int rackIndex, (int layer, Vector2 boardPos) tilePos)
        {
            this.RackIndex = rackIndex;
            this.TilePos = tilePos;
            this.TileId = tileId;
        }

        public void Execute()
        {
            // call select action logic here...
        }
    }
}