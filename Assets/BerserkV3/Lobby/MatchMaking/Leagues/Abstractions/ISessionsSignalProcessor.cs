using System;
using System.Threading;
using Berserk.Shared.SignalR.Enums;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Leagues
{
	public interface ILeagueSignalProcessor
	{
		event Action<LobbyLeagueAction, object> OnUpdateReceived;
		event Action OnConnectedSuccess;
		event Action OnReconnectedSuccess;
		event Action OnConnectionLost;
		
		UniTask SendAsync(object target, params object[] args);
		UniTask SendAsync(object target, CancellationToken token = default, params object[] args);
	}
}