namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IGiveCardsServiceFactory
	{
		IGiveCardsService Create(IGameContext gameContext, IGameLogicContext gameLogicContext);
	}
}