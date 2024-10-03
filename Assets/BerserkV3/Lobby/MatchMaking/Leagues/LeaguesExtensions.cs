using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.DataBase;
using BerserkV3.Lobby.Applications;
using BerserkV3.Lobby.Deck;

namespace BerserkV3.Lobby.MatchMaking.Leagues
{
	public static class LeaguesExtensions
	{
		public static bool IsDeckValid(this LeagueModel source, DeckData deck)
		{
			if (deck == null)
				return false;

			if (deck.OwnedCardIds.Count < SharedConfigAdapter.Config.MinCardsInDeck)
				return false;
			
			if (deck.OwnedCardIds.Count > SharedConfigAdapter.Config.MaxCardsInDeck)
				return false;

			if (!source.IsAllowed)
				return false;

			if (source.IsIgnoreRestrictions)
				return true;

			var ownedHero = VulcaniteHandler.Owned.FirstOrDefault(x => x.Id == deck.OwnedVulcaniteId);
			var vulcanite = GameDataBaseAdapter.Instance.GetHero(ownedHero?.VulcaniteId);
			
			if (vulcanite == null)
				return false;
			if (!deck.IsValid())
				return false;
			if (vulcanite.LevelAtSync > source.MaxLevel)
				return false;
			if (vulcanite.LevelAtSync < source.MinLevel)
				return false;

			var blackListVulcaniteIds = source.BlackListVulcaniteIds?.Split(",") ?? Array.Empty<string>();
			if (blackListVulcaniteIds.Contains(vulcanite.Id))
				return false;

			var fractionalCardCound = 0;
			var blackListCardIds = source.BlackListCardIds?.Split(",") ?? Array.Empty<string>();
			foreach (var cardEntityId in deck.OwnedCardIds)
			{
				var ownedCard = DeckApplicationAdapter.Application.OwnedCards.FirstOrDefault(x => x.Id == cardEntityId);
				var cardData = GameDataBaseAdapter.Instance.GetCard(ownedCard?.CardId);
				
				if (cardData == null || blackListCardIds.Contains(cardData.Id))
					return false;

				if (cardData.Quadrant == Quadrant.Neutral)
					continue;
				
				fractionalCardCound++;
				
				if ((int)cardData.Rarity > source.MaxTier)
					return false;
				if ((int)cardData.Rarity < source.MinTier)
					return false;
			}

			if (fractionalCardCound < source.MinFractionCards)
				return false;

			return true;
		}

		public static string GetDescription(this LeagueModel @this)
		{
			return @this.LeagueDescription.Replace("\\n", "\n").Split('\n')[0].Trim();
		}

		public static string GetArtURL(this LeagueModel @this)
		{
			return @this.LeagueDescription.Replace("\\n", "\n").Split('\n')[1].Trim();
		}
	}
}