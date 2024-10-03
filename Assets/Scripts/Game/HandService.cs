using System.Linq;
using Berserk.Shared.Data.Enums;
using Events;
using Game.Entities;
using CardData = Vulcan.Data.CardData;

namespace Game
{
	public class HandService
	{
		public void RemoveCards(params CardData[] cards)
		{
			RemoveCard(cards?.Select(card => GameBus.LocalContext
				                         .GetHandCardsByOwner(card.Owner)
				                         .FirstOrDefault(x => x.Data.UID == card.UID)).ToArray());
		}

		public void DealCards(params CardData[] cardsData)
		{
			GameBus.SpawnCardsInHand += cardsData;
		}

		public void RemoveCard(params HandCardEntity[] entities)
		{
			if(entities == null || entities.Length == 0)
				return;
			
			var rearrangeOwners = new Owner[entities.Length];
			for (var i = 0; i < entities.Length; i++)
			{
				var entity = entities[i];
				if (entity == null)
				{
					rearrangeOwners[i] = Owner.None;
					continue;
				}
				
				rearrangeOwners[i] = entity.Owner;
				GameBus.LocalContext.RemoveHandCard(entity);
				entity.Discard();
			}

			ReqestHandRearrange(rearrangeOwners);
		}

		private void ReqestHandRearrange(params Owner[] owners)
		{
			if(owners == null || owners.Length == 0)
				return;
			
			foreach (var owner in owners.Distinct())
			{
				if(owner == Owner.None)
					continue;
				
				GameBus.OnRequestHandRearrange += owner;
			}
		}
	}
}