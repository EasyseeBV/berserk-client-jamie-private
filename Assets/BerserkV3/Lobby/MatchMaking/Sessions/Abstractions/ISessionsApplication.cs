using Berserk.Shared.Data.Lobby.Matchmaking;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Sessions
{
	public interface ISessionsApplication
	{
		UniTask<bool> TryConnectGameAsync();
		UniTask<bool> ConnectGameAsync(ActiveSessionModel model);
	}
}