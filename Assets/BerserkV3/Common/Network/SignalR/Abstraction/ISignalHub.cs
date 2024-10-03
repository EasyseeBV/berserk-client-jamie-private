using System;
using System.Threading;
using System.Threading.Tasks;

namespace BerserkV3.Common.Network.SignalR
{
	public interface ISignalHub
	{
		event Action<string> OnError;
		event Action OnRestartRequired;
		event Action OnConnectedSuccess;
		event Action OnConnectionClosed;
		event Action OnReconnectedSuccess;
		event Action OnReconnectingAttempt;
		event Action<string, object[]> OnMessageReceived;

		string HubName { get; }
		
		Task ConnectByAcessTokenAsync(string authToken = "");

		void Close();

		void Reset();

		void Send(string target, params object[] args);

		Task SendAsync(string target, params object[] args);

		Task SendAsync(string target, CancellationToken token, params object[] args);
		
		void Reconnect(CancellationToken? token = null);
		
		void RestartRequired();
	}
}