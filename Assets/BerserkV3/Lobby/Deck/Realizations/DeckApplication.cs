using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.Lobby.Deck
{
	public class DeckApplication : IDeckApplication
	{
		public DeckData Current => User.ActiveDeck;
		public IEnumerable<DeckData> All => User.Decks;
		public IEnumerable<OwnedCard> OwnedCards => User.OwnedCards;

		public async UniTask InitActiveDeckAsync(DeckData newOne = null)
		{
			if (User.ActiveDeck == null)
				await SelectAsync(newOne ?? User.Decks.FirstOrDefault());
		}
		
		public async UniTask<bool> SaveAsync(DeckData deckModel)
		{
			var isNewDeck = string.IsNullOrEmpty(deckModel.Id);
			var result = isNewDeck
				? await DeckAPI.PostDeck(deckModel).AddLoadingTask()
				: await DeckAPI.PatchDeck(deckModel).AddLoadingTask();
			
			if (result == null) 
				return false;
			
			deckModel.Fill(result);
			User.Decks.RemoveAll(x => x.Id == deckModel.Id);
			User.Decks.Add(deckModel);
			await InitActiveDeckAsync(deckModel).AddLoadingTask();
			return true;
		}

		public async UniTask<bool> DeleteAsync(DeckData deckModel)
		{
			if (!await DeckAPI.DeleteDeck(deckModel).AddLoadingTask())
				return false;

			User.Decks.RemoveAll(x => x.Id == deckModel.Id);
			await InitActiveDeckAsync().AddLoadingTask();
			return true;
		}

		public async UniTask<bool> SelectAsync(DeckData deckModel)
		{
			if (deckModel == null)
				throw new NullReferenceException("Can't select missed deck.");
			
			await PatchDeckVulcaniteIfIsNull(deckModel).AddLoadingTask();
			var model = new DeckSelectModel {DeckId = deckModel.Id};
			var response = await DeckAPI.Select(model).AddLoadingTask();
			if (!response)
				throw new Exception(response.GetMessage());
			
			User.ActiveDeck = deckModel;
			return response;
		}

		public void RefreshSubscriptions(IEnumerable<string> expiredCardIds)
		{
			foreach (var ownedCard in OwnedCards.Where(x => expiredCardIds.Contains(x.Id)))
				ownedCard.IsSubscription = false;

			LobbyBus.OnUserDataRefreshed += true;
		}

		private async UniTask PatchDeckVulcaniteIfIsNull(DeckData deck, string vulcaniteEntityId = null)
		{
			if (deck == null)
				return;

			if (!string.IsNullOrEmpty(deck.OwnedVulcaniteId))
				return;
			
			deck.OwnedVulcaniteId = vulcaniteEntityId ?? User.OwnedVulcanites.FirstOrDefault(x => x.IsValid())?.Id;
			if (string.IsNullOrEmpty(deck.OwnedVulcaniteId))
			{
				RRLogger.Log($"[{GetType().Name.Orange()}] PatchDeckVulcaniteIfIsNull : The user does not have their own Vulcanite.");
				return;
			}

			RRLogger.Log($"[{GetType().Name.Orange()}] PatchDeckVulcaniteIfIsNull : {deck.OwnedVulcaniteId}");
			var deckResult = await DeckAPI.PatchDeck(deck);
			if (deckResult != null)
				deck.Fill(deckResult);
		}
	}
}