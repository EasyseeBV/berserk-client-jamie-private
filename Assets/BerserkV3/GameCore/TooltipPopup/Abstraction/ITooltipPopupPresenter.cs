using Berserk.Shared.Data.Abstraction;
using BerserkV3.GameCore.Cards.EffectHints;

namespace BerserkV3.GameCore.TooltipPopup
{
	public interface ITooltipPopupPresenter
	{
		void Setup(IEffectsHintsView view, IRuntimeData runtimeData, EffectOrigin origin);
		void Dispose();
	}
}