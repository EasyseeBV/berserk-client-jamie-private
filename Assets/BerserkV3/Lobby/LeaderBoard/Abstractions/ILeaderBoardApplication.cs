using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Berserk.Shared.Data.Lobby;

namespace BerserkV3.Lobby.LeaderBoard
{
	public interface ILeaderBoardApplication
	{
		UniTask<List<LeaderBoardScoreByLeagueModel>> GetLeagueLeaderBoard(string leagueId);
		UniTask<List<LeaderBoardTargetModel>> GetAllPlayers();
		UniTask<List<LeaderBoardFactionModel>> GetFactionsLeaderBoard();
		UniTask<Dictionary<string, List<LeaderBoardFactionModel>>> GetPlayersFactionsLeaderBoard();
		UniTask<List<object>> GetWhoBeatsWho();
	}
}