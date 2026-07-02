using Reflex.Attributes;

namespace TileMatch3.Core.PowerUp
{
    public class HintPower : BasePower
    {
        [Inject] HintSystem hintSystem;

        protected override void OnButtonClicked()
        {
            base.OnButtonClicked();
            hintSystem.ExecuteHint();
        }
    }
}