using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Controllers;

namespace Berserk.Shared.GameCore.LogicContext
{
	public class GiveCardsServiceFactory : IGiveCardsServiceFactory
	{
		public IGiveCardsService Create(IGameContext gameContext, IGameLogicContext gameLogicContext)
		{
			return gameContext.RuntimeData.MatchMode switch
			{
				MatchMode.Tutorial => new TutorialGiveCardsService(gameContext, gameLogicContext),
				_ => new DefaultGiveCardsService(gameContext, gameLogicContext)
			};
		}
	}
}