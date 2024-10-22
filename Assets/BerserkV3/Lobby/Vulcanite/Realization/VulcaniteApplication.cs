using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.UserInventory;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Decks;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.Vulcanite.Abstractions;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.Authorization.Inventory.Models;
using Lobby;
using RR.Core.Extensions;

namespace BerserkV3.Lobby.Vulcanite.Realization
{
	public class VulcaniteApplication : IVulcaniteApplication
	{
		private readonly IInventoryApplication inventoryApplication;
		
		public IEnumerable<OwnedVulcanite> Owned => inventoryApplication.Get<OwnedVulcanite>().ToList();

		public VulcaniteApplication(IInventoryApplication inventoryApplication)
		{
			this.inventoryApplication = inventoryApplication;
		}
		
		public OwnedVulcanite Get(string vulcaniteId) => Owned.FirstOrDefault(x => x.Id == vulcaniteId);

		public OwnedVulcanite GetFirstAvailable() => Owned.FirstOrDefault(x => x.IsValid());
		

		public async Task GiveVulcanite(string selectedId)
		{
			var response = await PlayerAPI.PatchSelectVulcanite(new SelectHeroDto { HeroId = selectedId }).AddLoadingTask();
			if (!response || response.Data == null)
				throw new Exception(response.GetMessage());

			inventoryApplication.Get<OwnedVulcanite>().Add(response.Data);
			
			foreach (var deck in DeckApplicationAdapter.Application.All.ToArray())
			{
				deck.OwnedVulcaniteId = response.Data.Id;
				await DeckApplicationAdapter.Application.SaveAsync(deck).AddLoadingTask();
			}

			await DeckApplicationAdapter.Application.SelectAsync(DeckApplicationAdapter.Application.All.FirstOrDefault()).AddLoadingTask();
		}

		public bool IsValidVulcanite(string vulcaniteId)
		{
			return inventoryApplication.Get<OwnedVulcanite>(vulcaniteId).IsValid();
		}

		public void RefreshVulcaniteRents(IEnumerable<string> expiredVulcaniteIds)
		{
			foreach (var ownedVulcanite in Owned.Where(x => expiredVulcaniteIds.Contains(x.Id)))
				ownedVulcanite.IsExpireRent = true;

			LobbyBus.OnUserDataRefreshed += true;
		}
	}
}