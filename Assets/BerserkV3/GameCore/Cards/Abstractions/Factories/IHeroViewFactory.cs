using Berserk.Shared.GameCore.Abstraction;

namespace BerserkV3.GameCore.Cards
{
	public interface IHeroViewFactory
	{
		IHeroView Create(IRuntimeGameObject runtimeGameObject);
	}
}