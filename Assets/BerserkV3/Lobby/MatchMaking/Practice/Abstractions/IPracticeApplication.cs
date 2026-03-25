using Cysharp.Threading.Tasks;
using Berserk.Shared.Data.Lobby;

namespace BerserkV3.Lobby.MatchMaking.Practice
{
	public interface IPracticeApplication
	{
		UniTask<bool> StartPracticeMatchAsync();
		UniTask<bool> StartTutorialMatchAsync();
		UniTask<bool> JoinGameAsync();
		UniTask<bool> JoinGameAsync(ActiveSessionModel model);
	}
}
