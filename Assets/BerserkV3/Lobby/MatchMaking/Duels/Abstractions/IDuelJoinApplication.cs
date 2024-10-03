using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public interface IDuelJoinApplication
	{
		void Open(string roomId, bool withRoomCode = false);
		UniTask JoinAsync(string roomId, string roomCode, string password);
	}
}