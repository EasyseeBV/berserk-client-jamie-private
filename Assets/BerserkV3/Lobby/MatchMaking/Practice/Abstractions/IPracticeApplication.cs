using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Practice
{
	public interface IPracticeApplication
	{
		UniTask<bool> StartPracticeMatchAsync();
		UniTask<bool> StartTutorialMatchAsync();
		UniTask<bool> JoinGameAsync();
	}
}