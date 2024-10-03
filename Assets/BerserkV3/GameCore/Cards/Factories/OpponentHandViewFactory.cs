using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.GameCore.Controllers;

namespace BerserkV3.GameCore.Cards
{

	public class OpponentHandViewFactory : TableBaseFactory, IOpponentHandViewFactory
	{
		private readonly IGameContainers gameContainers;
		protected override string ResourceName => "OpponentCardView";

		public OpponentHandViewFactory(IGameContainers gameContainers)
		{
			this.gameContainers = gameContainers;
		}

		public ICardView Create(IRuntimeGameObject runtimeGameObject)
		{
			return (ICardView) Create(runtimeGameObject, gameContainers.DeckOpponentContainer);
		}
	}

}