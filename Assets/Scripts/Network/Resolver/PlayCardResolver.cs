using System.Collections;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Events;
using Game;
using ServerCore.Infrastructure.Models;
using Vulcan.Data;
using Vulcan.Network.Context;

namespace Vulcan.Network.Resolver
{
	/// <summary>
	///	Resolve creeps cards from signal
	/// </summary>
	public static class PlayCardResolver
	{
		private static readonly HandService HAND_SERVICE = new HandService(); //TODO: DI
		public static void Resolve(PlayCardModel playCardModel, long utcTimeStamp)
		{
			var signalOwner = ActorsContextResolver.GetOwnerById(playCardModel.SessionPlayerId);
			GameBus.LocalContext.SetCardDeckCount(playCardModel.DeckCardsCount, signalOwner);

			foreach (var model in playCardModel.Cards)
			{
				EventQueue.Enqueue(new GameEvent(GameEventType.PlayCard,
				                                 CreateTableCard(model),
				                                 utcTimeStamp));
			}
			
			return;

			IEnumerator CreateTableCard(InteractiveCardModel model)
			{
				if (GameBus.CurrentRound.Value.TurnOwner == Owner.Opponent
				    && ActorsContextResolver.Opponent.IsControlledByAI)
					yield break;
				
				yield return null;

				var cardOwner = ResolverHelpers.GetOwnerByUsername(model.EntityState.OwnerUserName);
				var card = model.ToCardData(cardOwner);

				if (cardOwner == Owner.Opponent)
				{
					var handCards = GameBus.LocalContext.GetHandCardsByOwner(Owner.Opponent).ToArray();
					if(handCards.Length - playCardModel.HandCardsCount > 0)
						HAND_SERVICE.RemoveCard(handCards[0]);
				}
				
				// the table card does not need to be synchronized, because it came from the signal
				card.IsSpawnedByResolver = true; 
				GameBus.OnSpawnCard += card;
			}
		}
	}
}