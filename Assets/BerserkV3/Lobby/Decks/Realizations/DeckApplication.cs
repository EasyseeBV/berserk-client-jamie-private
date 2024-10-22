using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching;
using Berserk.Shared.Data.UserInventory;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.Vulcanite.Abstractions;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.Authorization.Inventory.Models;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Sirenix.Utilities;

namespace BerserkV3.Lobby.Decks
{
	public class DeckApplication : IDeckApplication
	{
		private readonly IInventoryApplication userInventory;
		private readonly IVulcaniteApplication vulcaniteApplication;

		public OwnedDeck Current
		{
			get => userInventory.Get<OwnedDeck>(User.Data?.LastDeckId);
			private set => User.Data.LastDeckId = value?.Id;
		}

		public IEnumerable<OwnedDeck> All => userInventory.Get<OwnedDeck>().ToList();
		public IEnumerable<OwnedCard> OwnedCards => userInventory.Get<OwnedCard>().ToList();
		public OwnedDeck ActiveDeck => userInventory.Get<OwnedDeck>(User.Data?.LastDeckId);

		public DeckApplication(
			IInventoryApplication userInventory,
			IVulcaniteApplication vulcaniteApplication)
		{
			this.userInventory = userInventory;
			this.vulcaniteApplication = vulcaniteApplication;
		}

		public async UniTask InitActiveDeckAsync(OwnedDeck newOne = null)
		{
			if (Current == null)
				await SelectAsync(newOne ?? All.FirstOrDefault());
		}

		public async UniTask<bool> SaveAsync(OwnedDeck ownedDeck)
		{
			var isNewDeck = string.IsNullOrEmpty(ownedDeck.Id);
			var result = isNewDeck
				? await DeckAPI.PostDeck(ownedDeck).AddLoadingTask()
				: await DeckAPI.PatchDeck(ownedDeck).AddLoadingTask();

			if (result == null)
				return false;

			ownedDeck.Fill(result);
			await InitActiveDeckAsync(ownedDeck).AddLoadingTask();
			return true;
		}

		public async UniTask<bool> DeleteAsync(OwnedDeck ownedDeck)
		{
			if (!await DeckAPI.DeleteDeck(ownedDeck).AddLoadingTask())
				return false;
			userInventory.Remove(ownedDeck);
			await InitActiveDeckAsync().AddLoadingTask();
			return true;
		}

		public async UniTask<bool> SelectAsync(OwnedDeck ownedDeck)
		{
			if (ownedDeck == null)
				throw new NullReferenceException("Can't select missed deck.");

			await PatchDeckVulcaniteIfIsNull(ownedDeck).AddLoadingTask();
			var model = new DeckSelectModel { DeckId = ownedDeck.Id };
			var response = await DeckAPI.Select(model).AddLoadingTask();
			if (!response)
				throw new Exception(response.GetMessage());

			Current = ownedDeck;
			return response;
		}

		public void RefreshSubscriptions(IEnumerable<string> expiredCardIds)
		{
			foreach (var ownedCard in OwnedCards.Where(x => expiredCardIds.Contains(x.Id)))
				ownedCard.IsSubscription = false;

			LobbyBus.OnUserDataRefreshed += true;
		}

		public bool IsValid(OwnedDeck ownedDeck)
		{
			var isValidVulcanite = vulcaniteApplication.IsValidVulcanite(ownedDeck?.OwnedVulcaniteId);

			return IsValidCards(ownedDeck) && isValidVulcanite;
		}

		public bool IsValidCards(OwnedDeck ownedDeck)
		{
			return ownedDeck != null
			       && !ownedDeck.OwnedCardIds.IsNullOrEmpty()
			       && OwnedCards
				       .Where(card => ownedDeck.OwnedCardIds.Contains(card.Id))
				       .All(x => x.IsValid());
		}

		private async UniTask PatchDeckVulcaniteIfIsNull(OwnedDeck ownedDeck, string vulcaniteEntityId = null)
		{
			if (ownedDeck == null)
				return;

			if (!string.IsNullOrEmpty(ownedDeck.OwnedVulcaniteId))
				return;

			ownedDeck.OwnedVulcaniteId = vulcaniteEntityId ?? userInventory.Get<OwnedVulcanite>().FirstOrDefault(x => x.IsValid())?.VulcaniteId;
			if (string.IsNullOrEmpty(ownedDeck.OwnedVulcaniteId))
			{
				RRLogger.Log($"[{GetType().Name.Orange()}] PatchDeckVulcaniteIfIsNull : The user does not have their own Vulcanite.");
				return;
			}

			RRLogger.Log($"[{GetType().Name.Orange()}] PatchDeckVulcaniteIfIsNull : {ownedDeck.OwnedVulcaniteId}");
			var deckResult = await DeckAPI.PatchDeck(ownedDeck);
			if (deckResult != null)
				ownedDeck.Fill(deckResult);
		}

		public bool IsValidForLeague(LeagueModel source, OwnedDeck ownedDeck)
		{
			if (ownedDeck == null)
				return false;

			if (ownedDeck.OwnedCardIds.Count < SharedConfigAdapter.Config.MinCardsInDeck)
				return false;

			if (ownedDeck.OwnedCardIds.Count > SharedConfigAdapter.Config.MaxCardsInDeck)
				return false;

			if (!source.IsAllowed)
				return false;

			if (source.IsIgnoreRestrictions)
				return true;

			var ownedHero = vulcaniteApplication.Get(ownedDeck.OwnedVulcaniteId);
			var vulcanite = GameDataBaseAdapter.Instance.GetHero(ownedHero?.VulcaniteId);

			if (vulcanite == null)
				return false;

			if (!IsValid(ownedDeck))
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
			foreach (var cardEntityId in ownedDeck.OwnedCardIds)
			{
				var ownedCard = OwnedCards.FirstOrDefault(x => x.Id == cardEntityId);
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
	}
}