using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.Data.UserInventory;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using RR.Core.DebugSystem;
using RR.Network.Rest;

namespace BerserkV3.Lobby.Network
{
	public class DeckAPI : API<DeckAPI>
	{
		public override string BaseUrl => URLs.APIUrl;
		public override string AuthToken => User.AccessToken;
		
		protected override void OnInit()
		{
			OnRequest += WriteLogSecure;
			OnResponse += WriteLogSecure;
		}

		private void WriteLogSecure(string message)
		{
			RRLogger.Warning(message);
		}
		
		public static async Task<APIResponse<string>> Select(DeckSelectModel model)
		{
			return await PostAsync<string>("Deck/Select", model);
		}

		public static async Task<List<OwnedDeck>> GetDecks()
		{
			return (await GetAsync<List<OwnedDeck>>("Deck")).Data;
		}

		public static async Task<OwnedDeck> PatchDeck(OwnedDeck ownedDeckPatchModel)
		{
			return (await PatchAsync<OwnedDeck>("Deck", ownedDeckPatchModel)).Data;
		}

		public static async Task<OwnedDeck> PostDeck(OwnedDeck ownedDeckPatchModel)
		{
			return (await PostAsync<OwnedDeck>("Deck", ownedDeckPatchModel)).Data;
		}

		public static async Task<bool> DeleteDeck(OwnedDeck ownedDeckPatchModel)
		{
			return await DeleteAsync<OwnedDeck>("Deck", ownedDeckPatchModel);
		}
	}
}