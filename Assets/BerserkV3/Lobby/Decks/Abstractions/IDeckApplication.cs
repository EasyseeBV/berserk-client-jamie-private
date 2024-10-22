using System.Collections.Generic;
using Berserk.Shared.Data.UserInventory;
using Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.Decks
{
	public interface IDeckApplication
	{
		OwnedDeck ActiveDeck { get; }
		OwnedDeck Current { get; }
		IEnumerable<OwnedDeck> All { get; }
		IEnumerable<OwnedCard> OwnedCards { get; }
		bool IsValid(OwnedDeck ownedDeck);
		bool IsValidForLeague(LeagueModel source, OwnedDeck ownedDeck);
		bool IsValidCards(OwnedDeck ownedDeck);
		
		UniTask InitActiveDeckAsync(OwnedDeck newOne = null);
		UniTask<bool> SaveAsync(OwnedDeck ownedDeckModel);
		UniTask<bool> DeleteAsync(OwnedDeck ownedDeckModel);
		UniTask<bool> SelectAsync(OwnedDeck ownedDeckModel);
		void RefreshSubscriptions(IEnumerable<string> expiredCardIds);
	}
}