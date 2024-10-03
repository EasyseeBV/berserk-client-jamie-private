using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.Deck
{
	public interface IDeckApplication
	{
		DeckData Current { get; }
		IEnumerable<DeckData> All { get; }
		IEnumerable<OwnedCard> OwnedCards { get; }
		
		UniTask InitActiveDeckAsync(DeckData newOne = null);
		UniTask<bool> SaveAsync(DeckData deckModel);
		UniTask<bool> DeleteAsync(DeckData deckModel);
		UniTask<bool> SelectAsync(DeckData deckModel);
		void RefreshSubscriptions(IEnumerable<string> expiredCardIds);
	}
}