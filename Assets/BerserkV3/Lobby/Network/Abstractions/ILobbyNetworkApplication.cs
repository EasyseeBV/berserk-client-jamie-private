using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.Network
{
	public interface ILobbyNetworkApplication
	{
		UniTask InitAsync();
	}
}