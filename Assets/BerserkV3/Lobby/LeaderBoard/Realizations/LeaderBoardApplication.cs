using System.Collections.Generic;
using Berserk.Shared.Data.Lobby;
using Cysharp.Threading.Tasks;
using BerserkV3.Lobby.Network;

namespace BerserkV3.Lobby.LeaderBoard
{
	public class LeaderBoardApplication : ILeaderBoardApplication
	{
		public async UniTask<List<LeaderBoardScoreByLeagueModel>> GetLeagueLeaderBoard(string leagueId)
		{
			var response = await LeaderBoardAPI.GetLeagueLeaderBoard(leagueId);
			return response != null && response.Data != null 
				? response.Data 
				: new List<LeaderBoardScoreByLeagueModel>();
		}

		public async UniTask<List<LeaderBoardTargetModel>> GetAllPlayers()
		{
			var response = await LeaderBoardAPI.GetAllPlayers();
			return response != null && response.Data != null
				? response.Data 
				: new List<LeaderBoardTargetModel>();
		}

		public async UniTask<List<LeaderBoardFactionModel>> GetFactionsLeaderBoard()
		{
			var response = await LeaderBoardAPI.GetFactionsLeaderBoard();
			return response != null && response.Data != null
				? response.Data
				: new List<LeaderBoardFactionModel>();
		}

		public async UniTask<Dictionary<string, List<LeaderBoardFactionModel>>> GetPlayersFactionsLeaderBoard()
		{
			var response = await LeaderBoardAPI.GetPlayersFactionsLeaderBoard();
			return response != null && response.Data != null
				? response.Data
				: new Dictionary<string, List<LeaderBoardFactionModel>>();
		}

		public async UniTask<List<object>> GetWhoBeatsWho()
		{
			var response = await LeaderBoardAPI.GetWhoBeatsWho();
			return response != null && response.Data != null
				? response.Data
				: new List<object>();
		}
	}
}
