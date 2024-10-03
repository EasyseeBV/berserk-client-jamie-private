using System;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Lobby.MatchMaking.Leagues.Data;
using BerserkV3.Lobby.UI.Leagues;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;
using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Leagues
{
	public interface ILeagueMatchSearchApplication
	{
		event Action OnCancel;
		event Action<bool> OnAccepted;
		event Action OnTimeOut;
		
		void StartSearch();
		void HideSearch();
		void FoundMatch(int countdown);
		void AcceptedMatch();
		void DeclinedOpponent(Action returnCallback, int countdown);
		void LoadingMatch();
		void LoadingStartQueue();
		void LoadingEndQueue();
		void LoadingDeclinedAndEndQueue();
		void ShowError(string message);
		void Disconnected();
	}

	public class LeagueMatchSearchApplication : IDisposable, IInitializable, ILeagueMatchSearchApplication
	{
		private const string TIME_BASE_FORMAT = "{0}<br><size=38>{1}</size> sec";
		private readonly ILobbyLeagueMatchSearchView view;
		private readonly IGameDatabase gameDatabase;
		private readonly MarchSearchModel model = new();
		private CancellationTokenSource timerCancellationTokenSource;
		
		public event Action OnCancel;
		public event Action<bool> OnAccepted;
		public event Action OnTimeOut;
		
		public LeagueMatchSearchApplication(
			ILobbyLeagueMatchSearchView view,
			IGameDatabase gameDatabase)
		{
			this.view = view;
			this.gameDatabase = gameDatabase;
		}
		
		public void Initialize()
		{
			model.OnStatusChanged += UpdateState;
			model.OnSearchTimeChanged += UpdateTimerText;
			model.OnTimerStateChahnged += UpdateTimerState;
		}		
		
		public void Dispose()
		{
			timerCancellationTokenSource?.Cancel();
			timerCancellationTokenSource?.Dispose();
			timerCancellationTokenSource = null;
			OnCancel = null;
			OnAccepted = null;
			OnTimeOut = null;
			model?.Dispose();
		}
		
		public void StartSearch()
		{
			if (model.State == MatchSearchState.Searching)
				return;
			
			model.State = MatchSearchState.Searching;
			model.IsTimerOn = true;
			model.IsTimerIncludeTimout = false;
			model.IsReverseTimer = false;
			model.Timer = 0;
			
			UpdateState(model.State);
			UpdateTimerState(model.IsTimerOn);
			Show();
		}
		
		public void HideSearch()
		{
			model.State = MatchSearchState.Nothing;
			model.IsTimerOn = false;
			model.IsTimerIncludeTimout = false;
			model.IsReverseTimer = false;
			model.Timer = 0;
			Close();
		}
		
		public void FoundMatch(int countdown)
		{
			model.State = MatchSearchState.Found;
			model.IsTimerOn = true;
			model.IsReverseTimer = true;
			model.Timer = countdown;
			model.IsTimerIncludeTimout = true;
			
			UpdateState(model.State);
			UpdateTimerState(model.IsTimerOn);
			Show();
		}

		public void AcceptedMatch()
		{
			model.State = MatchSearchState.Accepted;
			model.IsTimerOn = false;
			model.IsReverseTimer = false;
			model.Timer = 0;
			
			UpdateState(model.State);
			UpdateTimerState(model.IsTimerOn);
			Show();
		}
		
		public void DeclinedOpponent(Action returnCallback, int countdown)
		{
			if (model.State == MatchSearchState.DeclinedOpponent)
				return;
			
			model.State = MatchSearchState.DeclinedOpponent;
			model.IsTimerOn = true;
			model.IsTimerIncludeTimout = false;
			model.IsReverseTimer = true;
			model.Timer = countdown;
			model.OnSearchTimeChanged += TimerChanged;
			model.OnTimerStateChahnged += TimerStateChanged;
			model.OnStatusChanged += StatusChanged;
			
			UpdateState(model.State);
			UpdateTimerState(model.IsTimerOn);
			Show();
			
			return;

			void StatusChanged(MatchSearchState value)
				=> DeclineReturnCallback();
			
			void TimerChanged(int value)
			{
				if (value > countdown)
				{
					DeclineReturnCallback();
					return;
				}

				if (value > 0) 
					return;
				
				returnCallback?.Invoke();
				DeclineReturnCallback();
			}
			
			void TimerStateChanged(bool value)
				=> DeclineReturnCallback();
			
			void DeclineReturnCallback()
			{
				model.OnSearchTimeChanged -= TimerChanged;
				model.OnStatusChanged -= StatusChanged;
			}
		}

		public void LoadingMatch()
		{
			model.State = MatchSearchState.LoadingMatch;
			model.IsTimerOn = false;
			model.IsReverseTimer = false;
			model.Timer = 0;
			
			UpdateState(model.State);
			UpdateTimerState(model.IsTimerOn);
			Show();
		}

		public void LoadingStartQueue()
		{
			model.State = MatchSearchState.LoadingStartQueue;
			model.IsTimerOn = false;
			model.IsReverseTimer = false;
			model.Timer = 0;
			
			UpdateState(model.State);
			UpdateTimerState(model.IsTimerOn);
			Show();
		}

		public void LoadingEndQueue()
		{
			model.State = MatchSearchState.LoadingEndQueue;
			model.IsTimerOn = false;
			model.IsReverseTimer = false;
			model.Timer = 0;
			
			UpdateState(model.State);
			UpdateTimerState(model.IsTimerOn);
			Show();
		}

		public void LoadingDeclinedAndEndQueue()
		{
			model.State = MatchSearchState.LoadingDeclinedAndEndQueue;
			model.IsTimerOn = false;
			model.IsReverseTimer = false;
			model.Timer = 0;
			
			UpdateState(model.State);
			UpdateTimerState(model.IsTimerOn);
			Show();
		}

		public void ShowError(string message)
		{
			ConfirmationDialog.Instance.Init()
				.SetTitle("Information")
				.SetMessage(message)
				.Apply();
		}

		public void Disconnected()
		{
			model.State = MatchSearchState.Disconnected;
			model.IsTimerOn = false;
			model.IsReverseTimer = false;
			model.Timer = 0;
			
			UpdateState(model.State);
			UpdateTimerState(model.IsTimerOn);
			Show();
		}

		#region MyRegion

		private void Show()
		{
			if (view.IsVisible)
				return;

			view.SetCancelAction(() => OnCancel?.Invoke());
			view.SetAcceptAction(value => OnAccepted?.Invoke(value));
			view.Show();
		}

		private void Close()
		{
			if (view is not {IsVisible: true})
				return;
			
			view?.Close();
		}
		
		private void UpdateTimerText(int time)
		{
			if (time < 0)
			{
				view.SetTimerText(model.TimeText ?? string.Empty);
				return;
			}
			
			view.SetTimerText(model.IsTimerOn
				? string.Format(TIME_BASE_FORMAT, model.TimeText ?? string.Empty, time)
				: model.TimeText);
		}

		private void UpdateState(MatchSearchState state)
		{
			var titleText = "";
			view.SetCancelInteraction(false);
			view.SetAcceptButtonVisible(false);
			view.SetDeclineButtonVisible(false);
			view.SetCancelButtonVisible(false);
			
			switch (state)
			{
				case MatchSearchState.Searching:
					titleText = gameDatabase.GetLocalization("MatchSearchForAnOpponent");
					model.TimeText = gameDatabase.GetLocalization("MatchCurrWaiting");
					view.SetCancelInteraction(true);
					view.SetCancelButtonVisible(true);
					break;
				
				case MatchSearchState.Found:
					titleText = gameDatabase.GetLocalization("MatchAcceptingTitle");
					model.TimeText = gameDatabase.GetLocalization("MatchFoundDescription");
					view.SetAcceptButtonVisible(true);
					view.SetDeclineButtonVisible(true);
					break;
				
				case MatchSearchState.Accepted:
					titleText = gameDatabase.GetLocalization("MatchWaitingOpponentTitle");
					model.TimeText = gameDatabase.GetLocalization("MatchAcceptedDescription");
					break;
				
				case MatchSearchState.LoadingDeclinedAndEndQueue:
					titleText = gameDatabase.GetLocalization("MatchLoadingTitle");
					model.TimeText = gameDatabase.GetLocalization("MatchLoadingDeclinedAndQuitingDescription");
					break;
				
				case MatchSearchState.DeclinedOpponent:
					titleText = gameDatabase.GetLocalization("MatchLoadingTitle");
					model.TimeText = gameDatabase.GetLocalization("MatchDeclinedOpponentDescription");

					break;
				case MatchSearchState.LoadingEndQueue:
					titleText = gameDatabase.GetLocalization("MatchLoadingTitle");
					model.TimeText = gameDatabase.GetLocalization("MatchLoadingEndQueueDescription");
					break;
				
				case MatchSearchState.LoadingStartQueue:
					titleText = gameDatabase.GetLocalization("MatchLoadingTitle");
					model.TimeText = gameDatabase.GetLocalization("MatchLoadingStartQueueDescription");
					break;
				
				case MatchSearchState.LoadingMatch:
					titleText = gameDatabase.GetLocalization("MatchStartingTitle");
					model.TimeText = gameDatabase.GetLocalization("MatchStartingDescription");
					break;
				
				case MatchSearchState.Disconnected:
					titleText = gameDatabase.GetLocalization("MatchMakingDisconnectedTitle");
					model.TimeText = gameDatabase.GetLocalization("MatchMakingDisconnectedDescription");
					view.SetCancelInteraction(true);
					view.SetCancelButtonVisible(true);
					break;
			}
			
			view.SetTitleText(titleText);
			UpdateTimerText(model.Timer);
		}

		private void UpdateTimerState(bool isTimerOn)
		{
			timerCancellationTokenSource?.Cancel();
			timerCancellationTokenSource?.Dispose();
			timerCancellationTokenSource = null;

			if (!isTimerOn) 
				return;
			
			timerCancellationTokenSource = new CancellationTokenSource(); 
			UpdateTimer(timerCancellationTokenSource.Token).Forget();
		}
		
		private async UniTask UpdateTimer(CancellationToken token)
		{
			while (Application.isPlaying && !token.IsCancellationRequested)
			{
				var step = model.IsReverseTimer ? -1 : 1;
				model.Timer += step;
				if (model.Timer <= 0 && model.IsTimerIncludeTimout)
					TimeOut();
				await UniTask.Delay(1000, DelayType.Realtime, PlayerLoopTiming.Update, token);
			}
		}

		private void TimeOut()
		{
			OnTimeOut?.Invoke();
		}

		#endregion
	}
}