using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public interface IDuelsApplication
	{
		UniTask OpenAsync();
		void Close();
	}
}