using System;
using System.Threading;
using Berserk.Shared.SignalR.Enums;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.Network
{
	public interface ILobbySingalProcessor
	{
		event Action<SignalType, object> OnMessageReceived;

		UniTask SendAsync(object target, object data, CancellationToken token = default);
	}
}