using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Events;
using Game;
using ServerCore.Infrastructure.Models;
using UnityEngine;
using Vulcan.Data;
using Vulcan.Network.Context;
using CardData = Vulcan.Data.CardData;

namespace Vulcan.Network.Resolver
{
	/// <summary>
	///     Resolve from signal
	/// </summary>
	public static class AddCardResolver
	{
		private static readonly HandService HAND_SERVICE = new HandService(); //TODO: DI
		public static void Resolve(GameHandMessage signal, long timestamp)
		{
			EventQueue.Enqueue(new GameEvent(GameEventType.AddCard, ResolveRoutine(), timestamp));

			IEnumerator ResolveRoutine()
			{
				yield return null;
				var model = signal.SessionPlayerHandChangeModel;
				var isHiddenCards = RequestsResolver.SignalFromOpponent(signal);
				var owner = ActorsContextResolver.GetOwnerById(signal.SessionPlayerId);
				var cards = GetUserCards().ToArray();
				GameBus.LocalContext.SetCardDeckCount(model.DeckCardsCount, owner);
			
				if (cards.Any())
					GameBus.SpawnCardsInHand += cards;

				if (owner == Owner.Self)
					GameBus.LocalContext.AssignNextDeckCard(model.NextDeckCard.ToCardData(owner));

				IEnumerable<CardData> GetUserCards()
				{
					var remote = isHiddenCards
						? GetDummies(Mathf.Max(0, model.HandCardsCount - GameBus.LocalContext.GetHandCardsCount(owner)))
						: model.Cards;

					return remote.Select(x => x.ToCardData(owner));
				}

				IEnumerable<InteractiveCardModel> GetDummies(int count)
				{
					return count > 0 
						? Enumerable.Repeat(new InteractiveCardModel(), count)
						: Array.Empty<InteractiveCardModel>();
				}
			}
		}
	}
}