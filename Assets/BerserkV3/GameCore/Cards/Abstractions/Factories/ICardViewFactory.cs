using Berserk.Shared.GameCore.Abstraction;

namespace BerserkV3.GameCore.Cards
{

	public interface ICardViewFactory
	{
		ICardView Create(IRuntimeGameObject runtimeGameObject);
	}
}