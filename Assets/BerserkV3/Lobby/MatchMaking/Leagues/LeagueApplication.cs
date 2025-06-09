using System;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Lobby.Leagues;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.SignalR.Enums;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UnityEngine;
using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Leagues
{
	public class LeagueApplication : ILeagueApplication, IInitializable, IDisposable
	{
		private const int AUTO_REQUEST_STATE_MS = 10_000;
		private const int EXTRA_MS = 2_500;
		private readonly ISharedTime sharedTime;
		private readonly ISharedConfig sharedConfig;
		private readonly ILeagueMatchSearchApplication matchSearchApplication;
		private readonly ILeagueSignalProcessor signalProcessor;
		private CancellationTokenSource autoRequestStateSource;
		private bool isGoingToLeave;
		private bool isGoingToLeaveRaw;
		
		public LeagueApplication(
			ISharedTime sharedTime,
			ISharedConfig sharedConfig,
			ILeagueMatchSearchApplication matchSearchApplication,
			ILeagueSignalProcessor signalProcessor)
		{
			this.sharedTime = sharedTime;
			this.sharedConfig = sharedConfig;
			this.matchSearchApplication = matchSearchApplication;
			this.signalProcessor = signalProcessor;
		}

		public void Initialize()
		{
			signalProcessor.OnUpdateReceived += HandleServerSignals;
			signalProcessor.OnConnectedSuccess += RequestState;
			signalProcessor.OnConnectionLost += StopAutoRequestState;
			matchSearchApplication.OnCancel += LeaveSearch;
			matchSearchApplication.OnAccepted += AcceptedMatch;
			matchSearchApplication.OnTimeOut += OnAcceptTimeOut;
		}
		
		public void Dispose()
		{
			isGoingToLeaveRaw = isGoingToLeave = false;
			signalProcessor.OnUpdateReceived -= HandleServerSignals;
			signalProcessor.OnConnectedSuccess -= RequestState;
			signalProcessor.OnConnectionLost -= StopAutoRequestState;
			matchSearchApplication.OnCancel -= LeaveSearch;
			matchSearchApplication.OnAccepted -= AcceptedMatch;
			matchSearchApplication.OnTimeOut -= OnAcceptTimeOut;
			StopAutoRequestState();
		}

		#region OpenSearch

		public async UniTask OpenLeagueAsync(string leagueId, string deckId)
		{
			isGoingToLeaveRaw = isGoingToLeave = false;
			matchSearchApplication.LoadingStartQueue();
			await RequestToJoinAutoMatchAsync(leagueId, deckId);
		}

		private async UniTask RequestToJoinAutoMatchAsync(string leagueId, string deckId)
		{
			var model = new LeagueJoinAutoMatchModel
			{
				LeagueId = leagueId,
				DeckId = deckId
			};
			AutoRequestStateAsync(AUTO_REQUEST_STATE_MS).Forget();
			await signalProcessor.SendAsync(LobbyLeagueAction.LeagueJoinAutoMatch, model);
		}

		private void OpenSearchDialog()
		{
			OnInSearch();
			matchSearchApplication.StartSearch();
		}

		#endregion

		#region StopSearch

		private void LeaveSearch()
		{
			if (isGoingToLeaveRaw)
			{
				isGoingToLeaveRaw = false;
				HideSearchDialog();
				return;
			}
			matchSearchApplication.LoadingEndQueue();
			AutoRequestStateAsync(AUTO_REQUEST_STATE_MS).Forget();
			isGoingToLeave = true;
			signalProcessor.SendAsync(LobbyLeagueAction.LeagueLeaveAutoMatch).Forget();
		}

		private void HideSearchDialog()
		{
			OnSearchExit();
			StopAutoRequestState();
			matchSearchApplication.HideSearch();
			isGoingToLeave = isGoingToLeaveRaw = false;
		}

		#endregion

		#region AcceptOrDeclineMatch

		private void ReceiveAcceptMatch(LeagueMatchReadyToAcceptModel readyToAcceptModel)
		{
			OnInSearch();
			var delta = readyToAcceptModel.AcceptionTimeout.Subtract(sharedTime.Current).TotalSeconds;
			var totalSeconds = Mathf.Max(0, (int) delta);
			matchSearchApplication.FoundMatch(totalSeconds);
		}

		private async UniTask RequestAcceptMatchAsync(bool accepted)
		{
			var model = new LeagueMatchAcceptModel
			{
				Accepted = accepted
			};
			AutoRequestStateAsync(AUTO_REQUEST_STATE_MS).Forget();
			await signalProcessor.SendAsync(LobbyLeagueAction.LeagueMatchAccept, model);
		}

		private void AcceptionMatch(bool accepted)
		{
			OnInSearch();
			if (accepted)
			{
				matchSearchApplication.AcceptedMatch();
				return;
			}

			matchSearchApplication.LoadingDeclinedAndEndQueue();
		}
		
		private void AcceptedMatch(bool value)
		{
			AcceptionMatch(value);
			RequestAcceptMatchAsync(value).Forget();
		}
		
		private void OnAcceptTimeOut()
		{
			AcceptedMatch(true);
			//matchSearchApplication.LoadingDeclinedAndEndQueue();
		}

		private void MatchStarting()
		{
			matchSearchApplication.LoadingMatch();
		}

		private void Disconnected()
		{
			matchSearchApplication.Disconnected();
		}
		
		#endregion

		#region Common

		private void RequestState()
		{
			RequestStateAsync().Forget();
		}

		private UniTask RequestStateAsync(CancellationToken token = default)
		{
			return signalProcessor.SendAsync(LobbyLeagueAction.LeagueAutoMatchState, token);
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
			while (!token.IsCancellationRequested)
			{
				try
				{
					await Task.Delay(delayMS, token);
					if (isGoingToLeave)
						await Task.Delay(EXTRA_MS, token);
					
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
					if (maxError <= 0)
						LeaveSearch();
				}
			}
		}

		private void UpdateState(LeagueAutoMatchStateModel model)
		{
			isGoingToLeaveRaw = false;
			if (isGoingToLeave)
			{
				if (!model.IsMatchFound)
				{
					if (model.IsAutoMatchJoined)
					{
						LeaveSearch();
						return;
					}
				
					HideSearchDialog();
					return;
				}

				isGoingToLeave = false;
			}
			
			if (model.IsMatchFound)
			{
				if (model.IsMatchAccepted.HasValue)
				{
					AcceptionMatch(model.IsMatchAccepted.Value);
					return;
				}
				
				if (!model.AcceptionTimeOut.HasValue)
					return;
				
				var accept = new LeagueMatchReadyToAcceptModel
				{
					AcceptionTimeout = model.AcceptionTimeOut.Value
				};
				ReceiveAcceptMatch(accept);
				return;
			}
			
			if (model.IsAutoMatchJoined)
			{
				OpenSearchDialog();
				return;
			}
			
			HideSearchDialog();
		}

		private void ShowError(LeagueAutoMatchActionDeniedModel model)
		{
			matchSearchApplication.ShowError(model.Reason);
			RequestState();
		}

		private void OnInSearch()
		{
			signalProcessor.OnReconnectedSuccess -= RequestState;
			signalProcessor.OnReconnectedSuccess += RequestState;
		}

		private void OnSearchExit()
		{
			signalProcessor.OnReconnectedSuccess -= RequestState;
		}

		#endregion

		private void HandleServerSignals(LobbyLeagueAction action, object data)
		{
			switch (action)
			{
				case LobbyLeagueAction.LeagueJoinAutoMatch:
				{
					UpdateState((LeagueAutoMatchStateModel) data);
					AutoRequestStateAsync(AUTO_REQUEST_STATE_MS).Forget();
					break;
				}
				case LobbyLeagueAction.LeagueLeaveAutoMatch:
				{
					HideSearchDialog();
					break;
				}
				case LobbyLeagueAction.LeagueMatchAccept:
				{
					UpdateState((LeagueAutoMatchStateModel) data);
					AutoRequestStateAsync(sharedConfig.AutoMatchAcceptTimeoutMs + EXTRA_MS, 2).Forget();
					break;
				}
				case LobbyLeagueAction.LeagueMatchReadyToAccept:
				{
					ReceiveAcceptMatch((LeagueMatchReadyToAcceptModel) data);
					AutoRequestStateAsync(sharedConfig.AutoMatchAcceptTimeoutMs + EXTRA_MS, 2).Forget();
					break;
				}
				case LobbyLeagueAction.LeagueOpponentDeclinedMatch:
				{
					matchSearchApplication.DeclinedOpponent(() => UpdateState((LeagueAutoMatchStateModel) data), 3);
					AutoRequestStateAsync(AUTO_REQUEST_STATE_MS).Forget();
					break;
				}
				case LobbyLeagueAction.LeagueAutoMatchState:
				{
					UpdateState((LeagueAutoMatchStateModel) data);
					break;
				}
				case LobbyLeagueAction.LeagueAutoMatchActionDenied:
				{
					ShowError((LeagueAutoMatchActionDeniedModel) data);
					break;
				}
				case LobbyLeagueAction.LeagueMatchStarting:
				{
					MatchStarting();
					break;
				}
				case LobbyLeagueAction.LeagueLeaveAutoMatchDisconnected:
				{
					isGoingToLeaveRaw = true;
					Disconnected();
					break;
				}
				default: DefaultSharedLogger.Error($"[{GetType().Name.Orange().Bold()}] Not handled {nameof(LobbyLeagueAction)} : {action}");
					break;
			}
		}
	}
}