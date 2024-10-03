using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public interface IDuelCreateApplication
	{
		void Open();
		void Close();
		UniTask CreateAsync(string name, string password, bool isPrivate);
	}
}