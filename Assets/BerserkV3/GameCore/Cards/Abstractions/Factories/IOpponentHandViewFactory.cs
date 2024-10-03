using Berserk.Shared.GameCore.Abstraction;

namespace BerserkV3.GameCore.Cards
{

	public interface IOpponentHandViewFactory
	{
		ICardView Create(IRuntimeGameObject runtimeGameObject);
	}

}