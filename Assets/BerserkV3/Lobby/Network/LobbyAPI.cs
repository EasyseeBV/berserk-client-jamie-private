using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.Data.Lobby;
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
		
		public static async Task<APIResponse<ActiveSessionModel>> PostPracticeStartSession(LobbyPracticeStartSessionModel model)
		{
			return await PostAsync<ActiveSessionModel>("Lobby/PracticeStartSession", model);
		}
		
		public static async Task<APIResponse<LobbyDuelRoomModel[]>> GetDuelAvailableRooms()
		{
			return await GetAsync<LobbyDuelRoomModel[]>("Lobby/DuelAvailableRooms");
		}
		
		public static async Task<APIResponse<LobbyDuelRoomModel>> GetDuelActiveRoom()
		{
			return await GetAsync<LobbyDuelRoomModel>("Lobby/DuelActiveRoom");
		}
		
		public static async Task<APIResponse<LobbyDuelRoomModel>> PostDuelCreateRoom(LobbyCreateDuelRoomModel model)
		{
			return await PostAsync<LobbyDuelRoomModel>("Lobby/DuelCreateRoom", model);
		}
		
		public static async Task<APIResponse<string>> PostDuelStartSession()
		{
			return await PostAsync<string>("Lobby/DuelStartSession", jsonContent: null);
		}
		
		public static async Task<APIResponse<LobbyDuelRoomModel>> PostDuelJoinRoom(LobbyDuelJoinRoomModel model)
		{
			return await PostAsync<LobbyDuelRoomModel>("Lobby/DuelJoinRoom", model);
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