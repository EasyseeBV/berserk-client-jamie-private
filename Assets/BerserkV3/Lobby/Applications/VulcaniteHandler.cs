using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Authorization;
using Lobby;

namespace BerserkV3.Lobby.Applications
{
	//TODO Move or remove all data from class
	public static class VulcaniteHandler
	{
		public static IEnumerable<OwnedVulcanite> Owned => User.OwnedVulcanites;

		public static async Task GiveVulcanite(string selectedId)
		{
			var response = await PlayerAPI.PatchSelectVulcanite(new SelectHeroDto{HeroId = selectedId}).AddLoadingTask();
			if (!response || response.Data == null)
				throw new Exception(response.GetMessage());

			User.OwnedVulcanites.Add(response.Data);
			foreach (var deck in DeckApplicationAdapter.Application.All.ToArray())
			{
				deck.OwnedVulcaniteId = response.Data.Id;
				await DeckApplicationAdapter.Application.SaveAsync(deck).AddLoadingTask();
			}
			
			await DeckApplicationAdapter.Application.SelectAsync(DeckApplicationAdapter.Application.All.FirstOrDefault()).AddLoadingTask();
		}

		public static bool IsValidVulcanite(string vulcaniteId)
		{
			return Owned.FirstOrDefault(x => x.VulcaniteId == vulcaniteId).IsValid();
		}

		public static void RefreshVulcaniteRents(IEnumerable<string> expiredVulcaniteIds)
		{
			foreach (var ownedVulcanite in Owned.Where(x => expiredVulcaniteIds.Contains(x.Id)))
				ownedVulcanite.IsExpireRent = true;
			
			LobbyBus.OnUserDataRefreshed += true;
		}
	}
}