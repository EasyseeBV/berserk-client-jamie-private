using System;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.SignalR.Enums;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UI;
using UnityEngine;
using Zenject;

namespace BerserkV3.Lobby.MatchMaking.AutoMatching
{
	public class AutoMatchApplication : IAutoMatchApplication, IInitializable, IDisposable
	{
		private const int AUTO_REQUEST_STATE_MS = 10_000;
		private const int EXTRA_MS = 2_500;
		private readonly ISharedTime sharedTime;
		private readonly ISharedConfig sharedConfig;
		private readonly IGameDatabase gameDatabase;
		private readonly IAutoMatchSearchApplication matchSearchApplication;
		private readonly IAutoMatchAcceptApplication matchAcceptApplication;
		private readonly IAutoMatchSignalProcessor signalProcessor;
		private CancellationTokenSource autoRequestStateSource;

		public AutoMatchStateModel State { get; private set; } = new();
		public event Action OnStateChanged;

		public AutoMatchApplication(
			ISharedTime sharedTime,
			ISharedConfig sharedConfig,
			IGameDatabase gameDatabase,
			IAutoMatchSearchApplication matchSearchApplication,
			IAutoMatchAcceptApplication matchAcceptApplication,
			IAutoMatchSignalProcessor signalProcessor)
		{
			this.sharedTime = sharedTime;
			this.sharedConfig = sharedConfig;
			this.gameDatabase = gameDatabase;
			this.matchSearchApplication = matchSearchApplication;
			this.matchAcceptApplication = matchAcceptApplication;
			this.signalProcessor = signalProcessor;
		}

		public void Initialize()
		{
			signalProcessor.OnUpdateReceived += HandleServerSignals;
			signalProcessor.OnConnectedSuccess += RequestState;
			signalProcessor.OnConnectionLost += StopAutoRequestState;
			matchSearchApplication.OnCancel += LeaveAutoMatch;
			matchAcceptApplication.OnAccepted += SendAccept;
			matchAcceptApplication.OnTimeout += SetTimeoutState;
		}
		
		public void Dispose()
		{
			OnStateChanged = null;
			signalProcessor.OnUpdateReceived -= HandleServerSignals;
			signalProcessor.OnConnectedSuccess -= RequestState;
			signalProcessor.OnConnectionLost -= StopAutoRequestState;
			matchSearchApplication.OnCancel -= LeaveAutoMatch;
			matchAcceptApplication.OnAccepted -= SendAccept;
			matchAcceptApplication.OnTimeout -= SetTimeoutState;
			StopAutoRequestState();
		}
		
		public UniTask JoinAutoMatchAsync(AutoMatchJoinModel joinModel)
		{
			State = new AutoMatchStateModel
			{
				MatchMode = joinModel.MatchMode,
				IsAutoMatchJoined = true,
			};
			AutoRequestStateAsync(AUTO_REQUEST_STATE_MS).Forget();
			SetSearchingState();
			return signalProcessor.SendAsync(LobbyAutoMatchingAction.AutoMatchJoin, joinModel);
		}
		
		public void LeaveAutoMatch()
		{
			State.IsMarkedForAutoMatch = false;
			matchSearchApplication.Leaving();
			signalProcessor.SendAsync(LobbyAutoMatchingAction.AutoMatchLeave).Forget();
		}

		#region Visual Refresh
		
		private void SetSearchingState()
		{
			signalProcessor.OnReconnectedSuccess -= RequestState;
			signalProcessor.OnReconnectedSuccess += RequestState;
			matchSearchApplication.Searching();
			matchAcceptApplication.Close();
		}
		
		private void SetClosedState()
		{
			signalProcessor.OnReconnectedSuccess -= RequestState;
			StopAutoRequestState();
			matchSearchApplication.Stop();
			matchAcceptApplication.Close();
		}
		
		private void SetFoundState()
		{
			var timout = State.AcceptionTimeOut ?? sharedTime.Current;
			var delta = timout.Subtract(sharedTime.Current).TotalSeconds;
			var totalSeconds = Mathf.Max(0, (int) delta);
			
			matchSearchApplication.Accepting();
			matchAcceptApplication.Found(totalSeconds);
		}
		
		private void SetAcceptedState(bool accepted)
		{
			matchSearchApplication.Accepting();
			matchAcceptApplication.Accepted(accepted);
		}

		private void SetStartingState()
		{
			matchSearchApplication.Stop();
			matchAcceptApplication.Starting();
		}
		
		private void SetTimeoutState()
		{
			matchAcceptApplication.Timeout();
			matchSearchApplication.Leaving();
		}

		private void SetErrorState(string reason)
		{
			ConfirmationDialog.Instance.Init()
				.SetCancel()
				.SetOk(gameDatabase.GetLocalization("Client_AutoMatch_ErrorOkButton"))
				.SetMessage(reason)
				.SetTitle(gameDatabase.GetLocalization("Client_AutoMatch_ErrorTitle"))
				.Apply();
			RequestState();
		}

		#endregion

		#region Networking
		private void SendAccept(bool value)
		{
			SetAcceptedState(value);
			var model = new AutoMatchAcceptModel
			{
				MatchMode = State.MatchMode,
				Accepted = value
			};
			AutoRequestStateAsync(AUTO_REQUEST_STATE_MS).Forget();
			signalProcessor.SendAsync(LobbyAutoMatchingAction.AutoMatchAccept, model).Forget();
		}

		private void RequestState()
		{
			RequestStateAsync().Forget();
		}

		private UniTask RequestStateAsync(CancellationToken token = default)
		{
			return signalProcessor.SendAsync(LobbyAutoMatchingAction.AutoMatchState, token);
		}

		private void StopAutoRequestState()
		{
			autoRequestStateSource?.Cancel();
			autoRequestStateSource?.Dispose();
			autoRequestStateSource = null;
		}
		
		private async UniTask AutoRequestStateAsync(int delayMS, int maxError = 3)
		{
			autoRequestStateSource?.Cancel();
			autoRequestStateSource?.Dispose();
			autoRequestStateSource = new CancellationTokenSource();
			var token = autoRequestStateSource.Token;
			while (!token.IsCancellationRequested && Application.isPlaying)
			{
				try
				{
					await Task.Delay(delayMS, token);
					await RequestStateAsync(token).AttachExternalCancellation(token);
				}
				catch (OperationCanceledException)
				{
					return;
				}
				catch (Exception e)
				{
					maxError--;
					DefaultSharedLogger.Error(e);
					if (maxError > 0) 
						continue;
					
					LeaveAutoMatch();
					return;
				}
			}
		}

		private void UpdateState(AutoMatchStateModel model)
		{
			if (State.TimeStamp > model.TimeStamp)
				return;
			
			if (!State.Equals(model))
			{
				State = model;
				OnStateChanged?.Invoke();
			}
			
			if (model.IsMatchFound)
			{
				if (model.IsMatchAccepted.HasValue)
				{
					SetAcceptedState(model.IsMatchAccepted.Value);
					return;
				}
				
				if (!model.AcceptionTimeOut.HasValue)
					return;
				
				SetFoundState();
				return;
			}

			if (model.IsAutoMatchJoined)
			{
				SetSearchingState();
				return;
			}
			
			SetClosedState();
		}
		
		private void HandleServerSignals(LobbyAutoMatchingAction action, object data)
		{
			switch (action)
			{
				case LobbyAutoMatchingAction.AutoMatchJoin:
				{
					UpdateState((AutoMatchStateModel) data);
					AutoRequestStateAsync(AUTO_REQUEST_STATE_MS).Forget();
					break;
				}
				case LobbyAutoMatchingAction.AutoMatchLeave:
				{
					UpdateState((AutoMatchStateModel) data);
					break;
				}
				case LobbyAutoMatchingAction.AutoMatchAccept:
				{
					UpdateState((AutoMatchStateModel) data);
					AutoRequestStateAsync(sharedConfig.AutoMatchAcceptTimeoutMs + EXTRA_MS, 2).Forget();
					break;
				}
				case LobbyAutoMatchingAction.AutoMatchReadyToAccept:
				{
					UpdateState((AutoMatchStateModel) data);
					AutoRequestStateAsync(sharedConfig.AutoMatchAcceptTimeoutMs + EXTRA_MS, 2).Forget();
					break;
				}
				case LobbyAutoMatchingAction.AutoMatchOpponentDeclined:
				{
					matchSearchApplication.Searching();
					matchAcceptApplication.Return(() => UpdateState((AutoMatchStateModel) data));
					AutoRequestStateAsync(AUTO_REQUEST_STATE_MS).Forget();
					break;
				}
				case LobbyAutoMatchingAction.AutoMatchState:
				{
					UpdateState((AutoMatchStateModel) data);
					break;
				}
				case LobbyAutoMatchingAction.AutoMatchActionDenied:
				{
					SetErrorState(((AutoMatchActionDeniedModel) data).Reason);
					break;
				}
				case LobbyAutoMatchingAction.AutoMatchStarting:
				{
					SetStartingState();
					break;
				}
				case LobbyAutoMatchingAction.AutoMatchLeaveDisconnected:
				{
					SetErrorState(gameDatabase.GetLocalization("Client_AutoMatch_DisconnectedDescription"));
					break;
				}
				default: DefaultSharedLogger.Error($"[{GetType().Name.Orange().Bold()}] Not handled {nameof(LobbyAutoMatchingAction)} : {action}");
					break;
			}
		}
		
		#endregion
	}
}