using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.Network;
using RR.Core.DebugSystem;
using RR.Network.Rest;

namespace BerserkV3.Lobby.Network
{
	public class LeaderBoardAPI : API<LeaderBoardAPI>
	{
		public override string BaseUrl => URLs.APIUrl;

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
		
		public static async Task<APIResponse<List<PublicLeaderBoardScoreByLeagueModel>>> GetLeagueLeaderBoard(string leagueId)
		{
			var url = $"PublicLeaderBoard/LeagueLeaderBoard?leagueId={leagueId}";
			return await GetAsync<List<PublicLeaderBoardScoreByLeagueModel>>(url);
		}
		
		public static async Task<APIResponse<List<LeaderBoardTargetModel>>> GetAllPlayers()
		{
			return await GetAsync<List<LeaderBoardTargetModel>>("PublicLeaderBoard");
		}
		
		public static async Task<APIResponse<List<LeaderBoardFactionModel>>> GetFactionsLeaderBoard()
		{
			return await GetAsync<List<LeaderBoardFactionModel>>("PublicLeaderBoard/FactionsLeaderBoard");
		}
		
		public static async Task<APIResponse<Dictionary<string, List<LeaderBoardFactionModel>>>> GetPlayersFactionsLeaderBoard()
		{
			return await GetAsync<Dictionary<string, List<LeaderBoardFactionModel>>>("PublicLeaderBoard/PlayersFactionsLeaderBoard");
		}
		
		public static async Task<APIResponse<List<WhoBeatsWhoModel>>> GetWhoBeatsWho()
		{
			return await GetAsync<List<WhoBeatsWhoModel>>("PublicLeaderBoard/WhoBeatsWho");
		}
	}
}