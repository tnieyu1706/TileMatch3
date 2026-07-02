using System;

namespace TileMatch3.Core.CommandSystem
{
    public class MergeCommand : ICommand
    {
        public readonly RackActionInfo[] Actions;

        public readonly struct RackActionInfo
        {
            public readonly int RackIndex;
            public readonly Guid TileId;
            
            public RackActionInfo(int rackIndex, Guid tileId)
            {
                this.RackIndex = rackIndex;
                this.TileId = tileId;
            }
        }

        public MergeCommand(RackActionInfo[] actions)
        {
            this.Actions = actions;
        }
            
        public void Execute()
        {
            // call merge action logic here...
        }
    }
}