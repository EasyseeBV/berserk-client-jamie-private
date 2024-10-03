using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.Lobby.Abstractions;

namespace Berserk.Shared.Lobby.Deck
{
	public class DeckValueService : IDeckValueService
	{
		private readonly ISharedConfig sharedConfig;
		private readonly IGameDatabase gameDatabase;
		
		public DeckValueService(
			ISharedConfig sharedConfig,
			IGameDatabase gameDatabase)
		{
			this.sharedConfig = sharedConfig;
			this.gameDatabase = gameDatabase;
		}
		
		public float CalculateDeckValue(IEnumerable<OwnedCard> deckCards)
		{
			var rarityValues = sharedConfig.DeckRarityValueModels?.ToDictionary(x => x.Rarity, v => v.Value);

			if (rarityValues == null)
				return 0;
			
			float totalValue = 0;
			var nftCount = 0;
			
			foreach (var ownedCard in deckCards)
			{
				var cardData = gameDatabase.GetCard(ownedCard.CardId);
				nftCount += ownedCard.IsNft ? 1 : 0;
				totalValue += rarityValues[cardData.Rarity];
			}
			
			CalculateNftBonus(ref totalValue, nftCount);
			return totalValue;
		}
		
		private void CalculateNftBonus(ref float totalValue, int nftCount)
		{
			foreach (var nftBonus in sharedConfig.DeckBonusValueModels)
			{
				if (nftCount >= nftBonus.MinRange
				    && nftCount <= nftBonus.MaxRange)
				{
					totalValue += nftBonus.Value;
					if (nftCount == sharedConfig.MaxCardsInDeck) totalValue += totalValue;
					break;
				}
			}
		}
	}
}