using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.UI.Duels;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using RR.UI.FrameSystem;
using UI;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelCreateApplication : IDisposable, IDuelCreateApplication
	{
		private readonly IGameDatabase gameDatabase;
		private readonly IDeckApplication deckApplication;
		private readonly IDuelsSignalProcessor duelsSignalProcessor;
		private readonly ISignalTimeoutProcessor signalTimeoutProcessor;
		private static LobbyDuelCreateView Window => LobbyDuelCreateView.Instance;
		private readonly List<string> confirmationDialogIds = new();
		public DuelCreateApplication(
			IGameDatabase gameDatabase,
			IDeckApplication deckApplication,
			IDuelsSignalProcessor duelsSignalProcessor,
			ISignalTimeoutProcessor signalTimeoutProcessor)
		{
			this.gameDatabase = gameDatabase;
			this.deckApplication = deckApplication;
			this.duelsSignalProcessor = duelsSignalProcessor;
			this.signalTimeoutProcessor = signalTimeoutProcessor;
		}

		public void Dispose()
		{
			
			signalTimeoutProcessor.Enabled = false;
			signalTimeoutProcessor.Clear();
			
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			
			foreach (var dialogId in confirmationDialogIds.ToArray())
				ConfirmationDialog.Instance.Close(dialogId);

			confirmationDialogIds.Clear();
		}

		public void Open()
		{
			signalTimeoutProcessor.Clear();
			signalTimeoutProcessor.Enabled = true;
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			duelsSignalProcessor.OnUpdateReceived += OnMessageReceived;
			
			if (Window.VisibleState != VisibleState.Visible)
				Window.Show();
			
			Window.Clear();
			Window.SetCreateAction(() => CreateAsync(Window.Name, Window.Password, Window.IsPrivate).Forget());
			Window.SetCancelAction(Close);
		}

		public void Close()
		{
			signalTimeoutProcessor.Enabled = false;
			signalTimeoutProcessor.Clear();
			
			duelsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			if (Window.VisibleState == VisibleState.Visible)
				Window.Close();

			Window.Clear();
		}

		public async UniTask CreateAsync(string name, string password, bool isPrivate)
		{
			signalTimeoutProcessor.Enabled = true;
			var model = new LobbyCreateDuelRoomModel
			{
				Name = name.RemoveWhitespace(),
				Password = password.RemoveWhitespace(),
				MatchMode = MatchMode.Duel,
				Difficulty = PracticeMode.None,
				DeckId = deckApplication.Current?.Id,
				IsPrivate = isPrivate
			};
			
			var response = await LobbyAPI.PostDuelCreateRoom(model).AddLoadingTask();
			if (!response)
			{
				NotifyClientException(response.GetMessage());
				return;
			}
			
			if (!await signalTimeoutProcessor.WaitResponseAsync(LobbyDuelAction.Created, true).AddLoadingTask())
			{
				NotifyClientException(gameDatabase.GetLocalization("ClientDuels_CreateDuelFailed_NoResponse"),
					"Refresh", () => CreateAsync(name, password, isPrivate).AddLoadingTask().Forget(), true);
			}
		}
		
		private void NotifyClientException(string message, string okText = "OK", Action repeatAction = null, bool cancel = false)
		{
			var dialogData = ConfirmationDialog.Instance.Init();
			confirmationDialogIds.Add(dialogData.Key);
			dialogData
				.SetTitle("Information")
				.SetMessage(message)
				.SetResponseOk(repeatAction)
				.SetAnyResponse(() => confirmationDialogIds.Remove(dialogData.Key))
				.SetOk(okText)
				.SetCancel(cancel ? "Cancel" : null)
				.Apply();
		}
		
		private void OnMessageReceived(LobbyDuelAction action, LobbyDuelRoomModel duelModel)
		{
			switch (action)
			{
				case LobbyDuelAction.Created:
				{
					if (duelModel.Players.All(x => x.UserId != User.Id))
						return;
					
					signalTimeoutProcessor.ResponseReceived(action);
					Close();
					return;
				}
				case LobbyDuelAction.PlayerJoined:
				{
					if (duelModel.Players.All(x => x.UserId != User.Id))
						return;

					signalTimeoutProcessor.ResponseReceived(action);
					signalTimeoutProcessor.ResponseReceived(LobbyDuelAction.Created);
					Close();
					return;
				}
				case LobbyDuelAction.Started:
				{
					if (duelModel.Players.All(x => x.UserId != User.Id))
						return;
					
					signalTimeoutProcessor.ResponseReceived(action);
					signalTimeoutProcessor.ResponseReceived(LobbyDuelAction.Created);
					Close();
					return;
				}

				case LobbyDuelAction.Closed:
				case LobbyDuelAction.PlayerLeft:
				default: return;
			}
		}
	}
}