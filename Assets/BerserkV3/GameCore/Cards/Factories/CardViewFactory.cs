using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.GameCore.Controllers;
using RR.Core.DebugSystem;

namespace BerserkV3.GameCore.Cards
{
	public class CardViewFactory : TableBaseFactory, ICardViewFactory
	{
		private readonly IGameContainers gameContainers;
		protected override string ResourceName => "CardView";

		public CardViewFactory(IGameContainers gameContainers)
		{
			this.gameContainers = gameContainers;
		}

		public ICardView Create(IRuntimeGameObject runtimeGameObject)
		{
			if (runtimeGameObject is not IRuntimeGameCard)
			{
				RRLogger.Error($"You trying create not a card in Card factory!!! : {runtimeGameObject?.GetType().Name}");
				return default;
			}

			var parent = GameRepository.SelfId == runtimeGameObject.RuntimeData.OwnerUserId
				? gameContainers.DeckSelfContainer
				: gameContainers.DeckOpponentContainer;
			return (ICardView)base.Create(runtimeGameObject, parent);
		}
	}
}