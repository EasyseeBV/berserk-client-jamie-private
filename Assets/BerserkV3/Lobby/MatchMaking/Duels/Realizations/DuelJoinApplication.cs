using System;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Lobby.Matchmaking.Duels;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Decks;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.UI.Duels;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using RR.UI.FrameSystem;
using UI;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelJoinApplication : IDisposable, IDuelJoinApplication
	{
		private readonly IGameDatabase gameDatabase;
		private readonly IDeckApplication deckApplication;
		private readonly ISignalTimeoutProcessor signalTimeoutProcessor;
		private readonly IDuelsSignalProcessor duelsSignalProcessor;
		private static LobbyDuelJoinView Window => LobbyDuelJoinView.Instance;

		public DuelJoinApplication(
			IGameDatabase gameDatabase,
			IDeckApplication deckApplication,
			ISignalTimeoutProcessor signalTimeoutProcessor,
			IDuelsSignalProcessor duelsSignalProcessor)
		{
			this.gameDatabase = gameDatabase;
			this.deckApplication = deckApplication;
			this.signalTimeoutProcessor = signalTimeoutProcessor;
			this.duelsSignalProcessor = duelsSignalProcessor;
		}
		
		public void Dispose()
		{
			signalTimeoutProcessor.Enabled = false;
			signalTimeoutProcessor.Clear();
			
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			
			if (Window)
				Window.Clear();
		}
		
		public void Open(string roomId, bool withRoomCode = false)
		{
			if (Window.VisibleState != VisibleState.Visible)
				Window.Show();

			signalTimeoutProcessor.Clear();
			signalTimeoutProcessor.Enabled = true;
			
			Window.SetActiveRoomCode(withRoomCode);
			Window.SetJoinAction(() => JoinAsync(roomId, Window.RoomCode, Window.Password).AddLoadingTask().Forget());
			Window.SetCancelAction(Close);
		}

		public void Close()
		{
			signalTimeoutProcessor.Clear();
			signalTimeoutProcessor.Enabled = false;
			
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			if (Window.VisibleState == VisibleState.Visible)
				Window.Close();
			
			Window.Clear();
		}

		public async UniTask JoinAsync(string roomId, string roomCode, string password)
		{
			try
			{
				signalTimeoutProcessor.Enabled = true;
				duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
				duelsSignalProcessor.OnUpdateReceived += OnMessageReceived;
				var model = new DuelJoinRoomModel
				{
					RoomId = roomId.RemoveWhitespace(),
					RoomCode = roomCode.RemoveWhitespace(),
					Password = password.RemoveWhitespace(),
					DeckId = deckApplication.Current?.Id
				};

				var response = await LobbyAPI.PostDuelJoinRoom(model);
				if (!response)
				{
					NotifyClientException(response.GetMessage());
					return;
				}

				if (!await signalTimeoutProcessor.WaitResponseAsync(LobbyDuelAction.PlayerJoined, true))
				{
					NotifyClientException(gameDatabase.GetLocalization("ClientDuels_JoinDuelFailed_NoResponse"),
						"Try Again", () => JoinAsync(roomId, roomCode, password).AddLoadingTask().Forget(), true);
				}
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
			finally
			{
				duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			}
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

		private void OnMessageReceived(LobbyDuelAction action, DuelRoomModel roomModel)
		{
			if (action != LobbyDuelAction.PlayerJoined 
				|| roomModel.Players.All(x => x.UserId != User.Id))
				return;
			
			signalTimeoutProcessor.ResponseReceived(action);
			Close();
		}
	}
}