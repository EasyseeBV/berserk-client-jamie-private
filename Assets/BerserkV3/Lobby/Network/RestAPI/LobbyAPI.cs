using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.Data.Lobby.Matchmaking;
using Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching;
using Berserk.Shared.Data.Lobby.Matchmaking.Duels;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using RR.Core.DebugSystem;
using RR.Network.Rest;

namespace BerserkV3.Lobby.Network
{
	public class LobbyAPI : API<LobbyAPI>
	{
		public override string BaseUrl => URLs.APIUrl;
		public override string AuthToken => User.AccessToken;

		protected override void OnInit()
		{
			base.OnInit();
			OnRequest += WriteLogSecure;
			OnResponse += WriteLogSecure;
		}
		private void WriteLogSecure(string message)
		{
			RRLogger.Warning(message);
		}

		public static async Task<APIResponse<List<LeagueModel>>> GetAvailableLeagues()
		{
			return await GetAsync<List<LeagueModel>>("Lobby/AvailableLeagues");
		}
		
		public static async Task<APIResponse<ActiveSessionModel>> GetActiveSession()
		{
			return await GetAsync<ActiveSessionModel>("Lobby/ActiveSession");
		}
		
		public static async Task<APIResponse<ActiveSessionModel>> PostPracticeStartSession(LobbyJoinPracticeModel model)
		{
			return await PostAsync<ActiveSessionModel>("Lobby/PracticeStartSession", model);
		}
		
		public static async Task<APIResponse<DuelRoomModel[]>> GetDuelAvailableRooms()
		{
			return await GetAsync<DuelRoomModel[]>("Lobby/DuelAvailableRooms");
		}
		
		public static async Task<APIResponse<DuelRoomModel>> GetDuelActiveRoom()
		{
			return await GetAsync<DuelRoomModel>("Lobby/DuelActiveRoom");
		}
		
		public static async Task<APIResponse<DuelRoomModel>> PostDuelCreateRoom(DuelCreateRoomModel model)
		{
			return await PostAsync<DuelRoomModel>("Lobby/DuelCreateRoom", model);
		}
		
		public static async Task<APIResponse<string>> PostDuelStartSession()
		{
			return await PostAsync<string>("Lobby/DuelStartSession", jsonContent: null);
		}
		
		public static async Task<APIResponse<DuelRoomModel>> PostDuelJoinRoom(DuelJoinRoomModel model)
		{
			return await PostAsync<DuelRoomModel>("Lobby/DuelJoinRoom", model);
		}

		public static async Task<APIResponse<string>> PostDuelKickPlayer()
		{
			return await GetAsync<string>("Lobby/DuelKickPlayer");
		}

		public static async Task<APIResponse<ActiveSessionModel>> PostDuelLeaveRoom()
		{
			return await PostAsync<ActiveSessionModel>("Lobby/DuelLeaveRoom", jsonContent: null);
		}
	}
}