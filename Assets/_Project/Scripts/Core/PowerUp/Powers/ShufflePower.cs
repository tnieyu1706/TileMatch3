using Reflex.Attributes;
using TileMatch3.Core.BoardSystem;

namespace TileMatch3.Core.PowerUp
{
    public class ShufflePower : BasePower
    {
        [Inject] private BoardController boardController;

        protected override void OnButtonClicked()
        {
            base.OnButtonClicked();
            boardController.ShuffleBoard();
        }
    }
}