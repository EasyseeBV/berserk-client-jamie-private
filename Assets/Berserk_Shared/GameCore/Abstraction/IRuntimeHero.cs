using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimeHero : IRuntimeGameObject
	{
		new IRuntimeHeroData RuntimeData  { get; }
		new IHeroData Data { get; }
	}
}