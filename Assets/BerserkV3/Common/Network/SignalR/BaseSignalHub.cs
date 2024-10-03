using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.SignalR.Enums;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Messages;
using BestHTTP.WebSocket;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Network.SignalRCore;
using UnityEngine;

namespace BerserkV3.Common.Network.SignalR
{
	public abstract class BaseSignalHub : SignalHub, ISignalHub, IDisposable
	{
		protected virtual float ReconnectTimeMax => 10f;
		protected virtual float ReconnectOneTryTime => 1.5f;
		protected virtual float CloseAfterNoMessage => 5f;
		protected virtual bool EnableInternalReconnect => false;
		protected abstract string AuthToken { get; }
		protected abstract string ServerAddress  { get; }
		public abstract string HubName  { get; }
		
		private CancellationTokenSource reconnectTokenSource;
		private float? lastDisconnectTime;
		private bool lockReconnect;
		private bool isConnected;

		public event Action<string> OnError;
		public event Action OnRestartRequired;
		public event Action OnConnectedSuccess;
		public event Action OnConnectionClosed;
		public event Action OnReconnectedSuccess;
		public event Action OnReconnectingAttempt;
		public event Action<string, object[]> OnMessageReceived;

		public async Task ConnectByAcessTokenAsync(string authToken = "")
		{
			if (!Initialized)
				Build(ServerAddress, HubName);

			if (isConnected)
				return;
			
			await ConnectAsync(string.IsNullOrEmpty(authToken) ? AuthToken : authToken);
		}

		public new void Close()
		{
			reconnectTokenSource?.Cancel();
			reconnectTokenSource?.Dispose();
			reconnectTokenSource = null;
			lockReconnect = false;
			isConnected = false;
			lastDisconnectTime = 0;
			
			base.Close();
			Reset();
		}

		public virtual void Dispose()
		{
			reconnectTokenSource?.Cancel();
			reconnectTokenSource?.Dispose();
			reconnectTokenSource = null;
			OnError = null;
			OnConnectedSuccess = null;
			OnConnectionClosed = null;
			OnReconnectedSuccess = null;
			OnReconnectingAttempt = null;
			OnRestartRequired = null;
			OnMessageReceived = null;
			Close();
		}

		#region Handle Reqests

		public new void Send(string target, params object[] args)
		{
			LogSendRequest(target, args);
			base.Send(target, args);
		}

		public new Task SendAsync(string target, params object[] args)
		{
			LogSendRequest(target, args);
			return base.SendAsync(target, args);
		}

		public new Task SendAsync(string target, CancellationToken token, params object[] args)
		{
			LogSendRequest(target, args);
			return base.SendAsync(target, token, args);
		}

		private void LogSendRequest(object target, params object[] args)
		{
			var sendArgs = args.Where(x => x != null).Select(x => x as string ?? JsonConvert.SerializeObject(x, Formatting.Indented));
			DefaultSharedLogger.Log($"[{"SignalR".Orange().Bold()}][{HubName.ToUpper().Orange().Bold()}][{"Request".Pink().Bold()}] {target}\n{string.Join(",", sendArgs)}");
		}
		#endregion

		protected virtual void OverrideConnection(HubConnection connection)
		{
			if (connection == null)
				return;
			
			if(connection.Transport.TransportType != TransportTypes.WebSocket)
				return;

			var transportType = connection.Transport.GetType();
			var transportFields = transportType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			var connectionFields = connection.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			var websocketField = transportFields.FirstOrDefault(f => f.FieldType == typeof(WebSocket));
			var connectionReconnect = connectionFields.FirstOrDefault(f => f.Name == "defaultReconnect");

			if (connectionReconnect != null)
			{
				connectionReconnect.SetValue(connection, EnableInternalReconnect);
				RRLogger.Log($"Successful override field : HubConnection.defaultReconnect : {connectionReconnect.GetValue(connection)}");
			}
			
			if (websocketField == null)
			{
				RRLogger.Error("Cannot override Websocket field, target field not found");
				return;
			}

			if (websocketField.GetValue(connection.Transport) is not WebSocket webSocket)
			{
				RRLogger.Error($"Cannot override Websocket field, target field type not found, founded field with type : {websocketField.FieldType}");
				return;
			}
			
			webSocket.CloseAfterNoMessage = TimeSpan.FromSeconds(CloseAfterNoMessage);
			RRLogger.Log($"Successful override websocket field : WebSocket.CloseAfterNoMessage {webSocket.CloseAfterNoMessage}");
		}

		public virtual async void Reconnect(CancellationToken? token = null)
		{
			if (lockReconnect || !Application.isPlaying || token is {IsCancellationRequested: true})
				return;

			if (token == null)
			{
				reconnectTokenSource = new CancellationTokenSource();
				token = reconnectTokenSource.Token;
				
				if (reconnectTokenSource.IsCancellationRequested)
				{
					reconnectTokenSource?.Dispose();
					reconnectTokenSource = null;
					return;
				}
			}

			
			isConnected = false;
			lockReconnect = true;

			if (lastDisconnectTime.HasValue && lastDisconnectTime < Time.time) // Cant Reconnection
			{
				RestartRequired();
				return;
			}

			if (token.Value.IsCancellationRequested)
				return;
			
			Reset();
			Build(ServerAddress, HubName);
			ConnectByAcessTokenAsync().GetAwaiter();
			OnReconnectingAttempt?.Invoke();
			var reconnectStart = Time.time + ReconnectOneTryTime;
			while (Application.isPlaying && !token.Value.IsCancellationRequested)
			{
				await Task.Yield();
				if (isConnected)
				{
					lockReconnect = false;
					lastDisconnectTime = null;
					return;
				}

				if (!(Time.time > reconnectStart))
					continue;
				
				lastDisconnectTime ??= Time.time + ReconnectTimeMax;
				lockReconnect = false;
				Reconnect(token);
				return;
			}
		}

		protected override void OnClosed(HubConnection obj)
		{
			base.OnClosed(obj);
			isConnected = false;
			OnConnectionClosed?.Invoke();
		}

		protected override void OnConnected(HubConnection connection)
		{
			base.OnConnected(connection);
			
			isConnected = true;
			lastDisconnectTime = null;
			OverrideConnection(connection);
			OnConnectedSuccess?.Invoke();
		}

		protected override void OnReconnected(HubConnection obj)
		{
			base.OnReconnected(obj);
			OnReconnectedSuccess?.Invoke();
		}

		protected override void OnReconnecting(HubConnection arg1, string arg2)
		{
			base.OnReconnecting(arg1, arg2);
			Reconnect();
		}

		protected override void OnTransportEvent(HubConnection arg1, ITransport arg2, TransportEvents arg3)
		{
			base.OnTransportEvent(arg1, arg2, arg3);
			switch (arg3)
			{
				case TransportEvents.FailedToConnect:
				case TransportEvents.ClosedWithError: 
					Reconnect();
					break;
			}
		}

		protected override void HandleGenericError(string error)
		{
			base.HandleGenericError(error);
			Reconnect();
			OnErrorTrigger(error);
		}

		protected override void HandleConnectionFailure(string error)
		{
			base.HandleConnectionFailure(error);
			Reconnect();
			OnErrorTrigger(error);
		}

		protected override void HandleMessage(Message message)
		{
			if (!string.IsNullOrEmpty(message.target)
			    && message.target != $"{SignalType.Information}"
			    && message.target != $"{SignalType.Ping}")
				DefaultSharedLogger.Log($"[{"SignalR".Orange().Bold()}][{HubName.ToUpper().Orange().Bold()}][{"Response".Lightblue().Bold()}] {message.target}\n{string.Join(",", message.arguments)}");
			
			OnMessageReceived?.Invoke(message.target, message.arguments?.Where(x=> x != null).ToArray() ?? Array.Empty<object>());
		}

		protected void OnErrorTrigger(string value)
		{
			OnError?.Invoke(value);
		}

		public void RestartRequired()
		{
			Close();
			OnRestartRequired?.Invoke();
		}
	}
}