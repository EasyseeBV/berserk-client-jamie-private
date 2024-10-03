using BerserkV3.GameCore.EffectsVisual.Visuals;

namespace BerserkV3.GameCore.EffectsVisual.Abstractions
{
	public interface IVisualEffectsFactory
	{
		EffectVisual Create(IRuntimeEffectModel model);
	}
}