using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
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

		public static async Task<List<DeckData>> GetDecks()
		{
			return (await GetAsync<List<DeckData>>("Deck")).Data;
		}

		public static async Task<DeckData> PatchDeck(DeckData deckPatchModel)
		{
			return (await PatchAsync<DeckData>("Deck", deckPatchModel)).Data;
		}

		public static async Task<DeckData> PostDeck(DeckData deckPatchModel)
		{
			return (await PostAsync<DeckData>("Deck", deckPatchModel)).Data;
		}

		public static async Task<bool> DeleteDeck(DeckData deckPatchModel)
		{
			return await DeleteAsync<DeckData>("Deck", deckPatchModel);
		}
	}
}