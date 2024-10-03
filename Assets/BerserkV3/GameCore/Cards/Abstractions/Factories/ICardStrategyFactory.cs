using Berserk.Shared.Data.Enums;

namespace BerserkV3.GameCore.Cards
{
	public interface ICardStrategyFactory
	{
		ICardStrategy Create(RuntimeState state, ICardView view);
	}
}