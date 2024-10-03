using BerserkV3.GameCore.UI;

namespace BerserkV3.GameCore.Cards
{
	public interface IHeroView : IRuntimeObjectView
	{
		IEffectHintsApplication EffectHintsApplication { get; }
		IRuntimeLayout Layout { get; }
	}
}