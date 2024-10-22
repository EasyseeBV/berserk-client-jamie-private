using System;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Lobby.Matchmaking;
using Berserk.Shared.Data.Lobby.Matchmaking.Duels;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Common.AudioSystem;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.MatchMaking.Sessions;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.UI.Duels;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.UI.FrameSystem;
using UI;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelRoomApplication : IDisposable, IDuelRoomApplication
	{
		private readonly IAudioApplication audioApplication;
		private readonly ISessionsApplication sessionsApplication;
		private readonly ISignalTimeoutProcessor signalTimeoutProcessor;
		private readonly ISessionsSignalProcessor sessionsSignalProcessor;
		private readonly IDuelsSignalProcessor duelsSignalProcessor;
		private readonly ISharedConfig sharedConfig;
		private readonly IGameDatabase gameDatabase;

		private static LobbyDuelRoomView LobbyDuelRoomView => LobbyDuelRoomView.Instance;
		private DuelRoomItemData current;
		public event Action OnReturnRequested;
		
		public DuelRoomApplication(
			IAudioApplication audioApplication,
			ISessionsApplication sessionsApplication,
			ISignalTimeoutProcessor signalTimeoutProcessor,
			ISessionsSignalProcessor sessionsSignalProcessor,
			IDuelsSignalProcessor duelsSignalProcessor,
			ISharedConfig sharedConfig,
			IGameDatabase gameDatabase)
		{
			this.audioApplication = audioApplication;
			this.sessionsApplication = sessionsApplication;
			this.signalTimeoutProcessor = signalTimeoutProcessor;
			this.sessionsSignalProcessor = sessionsSignalProcessor;
			this.duelsSignalProcessor = duelsSignalProcessor;
			this.sharedConfig = sharedConfig;
			this.gameDatabase = gameDatabase;
		}

		public void Dispose()
		{
			signalTimeoutProcessor.Clear();
			signalTimeoutProcessor.Enabled = false;
			
			OnReturnRequested = null;
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			sessionsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
		}

		public async UniTask OpenAsync(DuelRoomItemData data, Action onReturn = null)
		{
			if (!await RefreshAsync(data))
				return;
			
			signalTimeoutProcessor.Clear();
			signalTimeoutProcessor.Enabled = true;
			
			if (LobbyDuelRoomView.VisibleState == VisibleState.Visible)
				return;
			
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			duelsSignalProcessor.OnUpdateReceived += OnMessageReceived;
			sessionsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			sessionsSignalProcessor.OnUpdateReceived += OnMessageReceived;
			OnReturnRequested += () => onReturn?.Invoke();
			LobbyDuelRoomView.Show();
		}
		
		public void Close()
		{
			signalTimeoutProcessor.Clear();
			signalTimeoutProcessor.Enabled = false;
			
			current?.Dispose();
			current = null;
			
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			sessionsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			
			if (LobbyDuelRoomView.VisibleState == VisibleState.Visible)
				LobbyDuelRoomView.Close();

			OnReturnRequested = null;
		}
		
		private void ReturnBack()
		{
			var mem = OnReturnRequested;
			Close();
			mem?.Invoke();
		}

		private async UniTask<bool> RefreshAsync(DuelRoomItemData data)
		{
			if (data == null || data.Players.All(p => p.UserId != User.Id) || data.IsClosed)
			{
				ReturnBack();
				return false;
			}
			
			if (current != data)
				current?.Dispose();
			
			current = data;
			await LobbyDuelRoomView.SetupAsync(current, User.Id);
			
			current.Players.ForEach(player => player
				.SetStartAction(OnPlayerStartDuelRequested)
				.SetKickAction(OnPlayerKickRequested)
				.SetLeaveAction(OnPlayerLeaveRequested));
			
			return true;
		}

		private async UniTask ReloadActiveRoomAsync()
		{
			var response = await LobbyAPI.GetDuelActiveRoom();
			var data = response.Data.MapDuelRoom(sharedConfig.PlayerLimit, 0);
			await RefreshAsync(data);
		}
		
		private async void OnPlayerStartDuelRequested(DuelPlayerItemData playerItemData)
		{
			var response = await LobbyAPI.PostDuelStartSession().AddLoadingTask();
			if (!response)
			{
				NotifyClientException(response.GetMessage());
				return;
			}
			
			TryConnectionSessionAsync().Forget();
		}

		private async void OnPlayerKickRequested(DuelPlayerItemData playerItemData)
		{
			var response = await LobbyAPI.PostDuelKickPlayer().AddLoadingTask();
			if (!response)
			{
				NotifyClientException(response.GetMessage());
				return;
			}

			var responseAction = playerItemData.IsHost ? LobbyDuelAction.Closed : LobbyDuelAction.PlayerLeft;
			if (!await signalTimeoutProcessor.WaitResponseAsync(responseAction, true).AddLoadingTask())
				NotifyClientException(gameDatabase.GetLocalization("ClientDuels_KickPlayerFailed_NoResponse"), 
					"Refresh", () => ReloadActiveRoomAsync().AddLoadingTask().Forget(), true);
		}

		private async void OnPlayerLeaveRequested(DuelPlayerItemData playerItemData)
		{
			var response = await LobbyAPI.PostDuelLeaveRoom().AddLoadingTask();
			if (!response)
			{
				NotifyClientException(response.GetMessage());
				return;
			}

			if (response.Data == null) // already left
			{
				ReturnBack();
				return;
			}
			
			var targetResponse = playerItemData.IsHost ? LobbyDuelAction.Closed : LobbyDuelAction.PlayerLeft;
			
			if (!await signalTimeoutProcessor.WaitResponseAsync(targetResponse, true).AddLoadingTask())
				NotifyClientException(gameDatabase.GetLocalization("ClientDuels_LeaveDuelFailed_NoResponse"), 
					"Refresh", () => ReloadActiveRoomAsync().AddLoadingTask().Forget(), true);
		}
		
		private void NotifyClientException(string message, string okText = "OK", Action repeatAction = null, bool cancel = false)
		{
			ConfirmationDialog.Instance.Init()
				.SetTitle("Information")
				.SetMessage(message)
				.SetResponseOk(repeatAction)
				.SetOk(okText)
				.SetCancel(cancel ? "Cancel" : null)
				.Apply();
		}

		#region Network
		
		private void OnMessageReceived(LobbyDuelAction action, DuelRoomModel roomModel)
		{
			switch (action)
			{
				case LobbyDuelAction.Closed:
				{
					if (current?.Id != roomModel.Id)
						return;
					
					signalTimeoutProcessor.ResponseReceived(action);
					if (roomModel.StartedDateTime.HasValue)
						return;
					
					var duelRoomData = roomModel.MapDuelRoom(sharedConfig.PlayerLimit, 0);
					RefreshAsync(duelRoomData).Forget();
					return;
				}
				case LobbyDuelAction.PlayerJoined or LobbyDuelAction.PlayerLeft:
				{
					if (current?.Id != roomModel.Id)
						return;
					
					signalTimeoutProcessor.ResponseReceived(action);
					var duelRoomData = roomModel.MapDuelRoom(sharedConfig.PlayerLimit, 0);
					RefreshAsync(duelRoomData).Forget();
					
					if (duelRoomData.HostId == User.Id)
						audioApplication.PlaySound(Clip.Lobby_OpponentEntered);
					
					return;
				}
				case LobbyDuelAction.Started:
				{
					if (current?.Id != roomModel.Id)
						return;

					signalTimeoutProcessor.ResponseReceived(action);
					
					if (roomModel.HostId == User.Id)
						return;
					
					TryConnectionSessionAsync().Forget();
					return;
				}
				
				default: return;
			}
		}

		private async UniTask TryConnectionSessionAsync()
		{
			if (current is {IsStarted: true})
				return;
			
			if (await signalTimeoutProcessor.WaitResponseAsync(LobbySessionsAction.ActiveSession, true).AddLoadingTask())
				return;
				
			NotifyClientException(gameDatabase.GetLocalization("ClientDuels_StartDuelFailed_NoResponse"), 
				"Try Again", TryForceConnectSessionAsync);
			
			return;
			async void TryForceConnectSessionAsync()
			{
				if (await sessionsApplication.TryConnectGameAsync().AddLoadingTask())
					return;

				Close();
				LobbyBus.OnReconnectRequired.Publish();
			}
		}
		
		private void OnMessageReceived(LobbySessionsAction action, ActiveSessionModel model)
		{
			if (action != LobbySessionsAction.ActiveSession
			    || model.Players.All(x => x.UserId != User.Id))
				return;

			if (current != null)
				current.IsStarted = true;
			signalTimeoutProcessor.ResponseReceived(action);
		}
		#endregion
	}
}