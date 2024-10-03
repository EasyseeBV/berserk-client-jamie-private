using System;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using BestHTTP.SignalRCore.Messages;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Path = System.IO.Path;

namespace RR.Network.SignalRCore
{
	public abstract class SignalHub
	{
		private HubConnection connection;
		private string serverAddress;
		private string hubName;

		protected bool Initialized => connection != null;

		public SignalHub Build(string serverAddress, string hubName)
		{
			if (Initialized)
			{
				RRLogger.Log($"[{GetType().Name.Orange().Bold()}] : Already Initialized!");
				return this;
			}
			
			this.serverAddress = serverAddress;
			this.hubName = hubName;

			var hubAddress = Path.Combine(serverAddress, hubName);
			connection = new HubConnection(new Uri(hubAddress), new JsonProtocol(new JsonDotNetEncoder()))
			{
				ReconnectPolicy = new DefaultRetryPolicy()
			};

			connection.OnMessage += OnMessageReceived;
			connection.OnConnected += OnConnected;
			connection.OnError += OnError;
			connection.OnClosed += OnClosed;
			connection.OnReconnected += OnReconnected;
			connection.OnReconnecting += OnReconnecting;
			connection.OnTransportEvent += OnTransportEvent;

			return this;
		}
		
		public void Reset()
		{
			if (!Initialized)
			{
				RRLogger.Log($"[{GetType().Name.Orange().Bold()}] : Not Initialized!");
				return;
			}
			
			if (connection != null)
			{
				connection.OnMessage -= OnMessageReceived;
				connection.OnConnected -= OnConnected;
				connection.OnError -= OnError;
				connection.OnClosed -= OnClosed;
				connection.OnReconnected -= OnReconnected;
				connection.OnReconnecting -= OnReconnecting;
				connection.OnTransportEvent -= OnTransportEvent;
				connection.StartClose();
			}
			
			connection = null;
			serverAddress = null;
			hubName = null;
		}

		public void Connect(string accessToken)
		{
			if (string.IsNullOrEmpty(serverAddress))
			{
				RRLogger.Error($"{nameof(serverAddress)} can't be {nameof(string.IsNullOrEmpty)}");
				return;
			}

			if (string.IsNullOrEmpty(hubName))
			{
				RRLogger.Error($"{nameof(hubName)} can't be {nameof(string.IsNullOrEmpty)}");
				return;
			}

			if (connection == null)
			{
				RRLogger.Error($"{nameof(connection)} is NULL. Use fluent: {nameof(Initialize)}.{nameof(Connect)}");
				return;
			}

			connection.AuthenticationProvider = new HeaderAuthenticator(accessToken, connection);
			connection.StartConnect();
		}
		public async Task ConnectAsync(string accessToken)
		{
			if (string.IsNullOrEmpty(serverAddress))
			{
				RRLogger.Error($"{nameof(serverAddress)} can't be {nameof(string.IsNullOrEmpty)}");
				return;
			}

			if (string.IsNullOrEmpty(hubName))
			{
				RRLogger.Error($"{nameof(hubName)} can't be {nameof(string.IsNullOrEmpty)}");
				return;
			}

			if (connection == null)
			{
				RRLogger.Error($"{nameof(connection)} is NULL. Use fluent: {nameof(Initialize)}.{nameof(Connect)}");
				return;
			}

			connection.AuthenticationProvider = new HeaderAuthenticator(accessToken, connection);
			await connection.ConnectAsync();
		}

		public void Close()
		{
			if (!Initialized)
			{
				RRLogger.Log($"[{GetType().Name.Orange().Bold()}] : not initialized !");
				return;
			}
			connection.StartClose();
		}

		public async Task CloseAsync()
		{
			if (!Initialized)
			{
				RRLogger.Log($"[{GetType().Name.Orange().Bold()}] : not initialized !");
				return;
			}
			await connection.CloseAsync();
		}

		public void Send(string target, params object[] @params)
		{
			if (!Initialized)
			{
				RRLogger.Log($"[{GetType().Name.Orange().Bold()}] : not initialized !");
				return;
			}
			connection.Send(target, @params);
		}

		public async Task SendAsync(string target, params object[] @params)
		{
			if (!Initialized)
			{
				RRLogger.Log($"[{GetType().Name.Orange().Bold()}] : not initialized !");
				return;
			}
			await connection.SendAsync(target, @params);
		}

		public async Task SendAsync(string target, CancellationToken token, params object[] @params)
		{
			if (!Initialized)
			{
				RRLogger.Log($"[{GetType().Name.Orange().Bold()}] : not initialized !");
				return;
			}
			await connection.SendAsync(target, token, @params);
		}

		private bool OnMessageReceived(HubConnection connection, Message message)
		{
			if (message.target == "Error")
				RRLogger.Error($"[{hubName.Pink()}] Error: {message.arguments[0]}");

			HandleMessage(message);
			return true;
		}

		private void OnError(HubConnection connection, string error)
		{
			RRLogger.Error($"[{hubName.Pink()}] Error: {error}");

			if (connection.State == ConnectionStates.CloseInitiated || connection.State == ConnectionStates.Closed)
				HandleConnectionFailure(error);
			else
				HandleGenericError(error);
		}

		protected virtual void OnConnected(HubConnection connection)
		{
			RRLogger.Log($"[{hubName.Pink()}] Connected");
		}

		protected virtual void OnTransportEvent(HubConnection arg1, ITransport arg2, TransportEvents arg3)
		{
			RRLogger.Log($"[{hubName.Pink()}] OnTransportEvent: {arg2} | {arg3}");
		}

		protected virtual void OnReconnecting(HubConnection arg1, string arg2)
		{
			RRLogger.Log($"[{hubName.Pink()}] {nameof(OnReconnecting).Gray()}... {arg2}");
		}

		protected virtual void OnReconnected(HubConnection obj)
		{
			RRLogger.Log($"[{hubName.Pink()}] {nameof(OnReconnected).Green()}");
		}

		protected virtual void OnClosed(HubConnection obj)
		{
			RRLogger.Log($"[{hubName.Pink()}] {nameof(OnClosed).Orange()}");
		}

		protected virtual void HandleGenericError(string error) { }
		protected virtual void HandleConnectionFailure(string error) { }

		protected abstract void HandleMessage(Message message);

		// TODO: remove after migrating old projects to the last RR version
		#region Legacy

		public void Open(string accessToken) => Connect(accessToken);
		public SignalHub Initialize(string serverAddress, string hubName) => Build(serverAddress, hubName);

		public void RetryOpen()
		{ /*Do nothing*/
			RRLogger.Error($"REMOVED IN SIGNALR-CORE - Using auto-reconnect policy: {connection.ReconnectPolicy?.GetType().Name}");
		}

		public void ResetRetries()
		{
			/*Do nothing*/
			RRLogger.Error($"REMOVED IN SIGNALR-CORE - Using auto-reconnect policy: {connection.ReconnectPolicy?.GetType().Name}");
		}

		#endregion Legacy
	}
}