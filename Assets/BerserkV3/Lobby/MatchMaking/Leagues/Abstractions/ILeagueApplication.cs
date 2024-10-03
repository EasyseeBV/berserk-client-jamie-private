using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Leagues
{
	public interface ILeagueApplication
	{
		UniTask OpenLeagueAsync(string leagueId, string deckId);
	}
}