using System.Linq;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Lobby.Applications;
using Sirenix.Utilities;

namespace BerserkV3.Lobby.Deck
{
	public static class DeckExtensions
	{
		public static bool IsValid(this DeckData deckModel)
		{
			return deckModel.IsValidCards()
			       && deckModel.IsValidHero();
		}
		
		public static bool IsValidCards(this DeckData deckModel)
		{
			return deckModel != null
			       && !deckModel.OwnedCardIds.IsNullOrEmpty()
			       && DeckApplicationAdapter.Application.OwnedCards
				       .Where(card => deckModel.OwnedCardIds.Contains(card.Id))
				       .All(x => x.IsValid());
		}

		public static bool IsValidHero(this DeckData deckData)
		{
			return VulcaniteHandler.Owned.FirstOrDefault(x => x.Id == deckData?.OwnedVulcaniteId).IsValid();
		}
	}
}