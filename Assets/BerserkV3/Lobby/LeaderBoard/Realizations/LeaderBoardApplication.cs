using System.Collections.Generic;
using Berserk.Shared.Data.Lobby;
using Cysharp.Threading.Tasks;
using BerserkV3.Lobby.Network;

namespace BerserkV3.Lobby.LeaderBoard
{
	public class LeaderBoardApplication : ILeaderBoardApplication
	{
		public async UniTask<List<PublicLeaderBoardScoreByLeagueModel>> GetLeagueLeaderBoard(string leagueId)
		{
			var response = await LeaderBoardAPI.GetLeagueLeaderBoard(leagueId);
			return response.Data != null 
				? response.Data 
				: new List<PublicLeaderBoardScoreByLeagueModel>();
		}

		public async UniTask<List<LeaderBoardTargetModel>> GetAllPlayers()
		{
			var response = await LeaderBoardAPI.GetAllPlayers();
			return response.Data != null
				? response.Data 
				: new List<LeaderBoardTargetModel>();
		}

		public async UniTask<List<LeaderBoardFactionModel>> GetFactionsLeaderBoard()
		{
			var response = await LeaderBoardAPI.GetFactionsLeaderBoard();
			return response.Data != null
				? response.Data
				: new List<LeaderBoardFactionModel>();
		}

		public async UniTask<Dictionary<string, List<LeaderBoardFactionModel>>> GetPlayersFactionsLeaderBoard()
		{
			var response = await LeaderBoardAPI.GetPlayersFactionsLeaderBoard();
			return response.Data != null
				? response.Data
				: new Dictionary<string, List<LeaderBoardFactionModel>>();
		}

		public async UniTask<List<WhoBeatsWhoModel>> GetWhoBeatsWho(int? limit = null)
		{
			var response = await LeaderBoardAPI.GetWhoBeatsWho(limit);
			return response.Data != null
				? response.Data
				: new List<WhoBeatsWhoModel>();
		}
		
		public async UniTask<List<WhoBeatsWhoModel>> GetWhoBeatsWhoByPlayer(string playerUsername, int? limit = null)
		{
			var response = await LeaderBoardAPI.GetWhoBeatsWhoByPlayer(playerUsername,  limit);
			return response.Data != null
				? response.Data
				: new List<WhoBeatsWhoModel>();
		}
		
		public async UniTask<List<PublicLeaderBoardScoreByLeagueModel>> GetLeagueLeaderBoardBySeason(string leagueId, string seasonId)
		{
			var response = await LeaderBoardAPI.GetLeagueLeaderBoardBySeason(leagueId, seasonId);
			return response.Data != null
				? response.Data
				: new List<PublicLeaderBoardScoreByLeagueModel>();
		}
	}
}
