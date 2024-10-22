using System;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Lobby.UI.MatchMaking;
using Cysharp.Threading.Tasks;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Lobby.MatchMaking.AutoMatching
{
	public class AutoMatchAcceptViewModel
	{
		public CancellationToken Token => stateSource?.Token ?? CancellationToken.None;
		public bool IsShowed { get; set; }
		public bool AcceptVisible { get; set; }
		public bool DeclineVisible { get; set; }
		public string HeaderLocKey { get; set; }
		public string MessageLocKey { get; set; }
		public bool TimerReverse { get; set; }
		public int? Timer { get; set; }
		public Action Timout { get; set; }

		private CancellationTokenSource stateSource;

		public void PrepareState()
		{
			Clear();
			stateSource = new CancellationTokenSource();
		}

		public void Clear()
		{
			stateSource?.Cancel();
			stateSource?.Dispose();
			stateSource = null;
			Timout = null;
			Timer = null;
			MessageLocKey = null;
			HeaderLocKey = null;
			AcceptVisible = false;
			DeclineVisible = false;
			TimerReverse = false;
		}
	}

	public class AutoMatchAcceptApplication : IAutoMatchAcceptApplication, IDisposable
	{
		private readonly IUIService uiService;
		private readonly IGameDatabase gameDatabase;
		private readonly AutoMatchAcceptViewModel model;
		
		public event Action<bool> OnAccepted;
		public event Action OnTimeout;

		public AutoMatchAcceptApplication(
			IUIService uiService,
			IGameDatabase gameDatabase)
		{
			this.uiService = uiService;
			this.gameDatabase = gameDatabase;
			model = new AutoMatchAcceptViewModel();
		}

		public void Dispose()
		{
			model.Clear();
			OnAccepted = null;
			OnTimeout = null;
		}

		public void Close()
		{
			if (!model.IsShowed)
				return;
			
			model.Clear();
			model.IsShowed = false;
			uiService.Begin<AutoMatchAcceptWindow>().Hide();
		}

		public void Found(int countdown)
		{
			model.PrepareState();
			model.AcceptVisible = true;
			model.DeclineVisible = true;
			model.HeaderLocKey = "Client_AutoMatch_Accept_MatchFoundTitle";
			model.Timer = countdown;
			model.Timout = () => OnTimeout?.Invoke();
			model.TimerReverse = true;
			UpdateState();
		}

		public void Accepted(bool value)
		{
			var locKey = value ? "Accepted" : "Declined";
			model.PrepareState();
			model.HeaderLocKey = $"Client_AutoMatch_Accept_{locKey}Title";
			model.MessageLocKey = $"Client_AutoMatch_Accept_{locKey}Description";
			UpdateState();
		}

		public void Starting()
		{
			model.PrepareState();
			model.HeaderLocKey = "Client_AutoMatch_Accept_StartedTitle";
			model.MessageLocKey = "Client_AutoMatch_Accept_StartedDescription";
			UpdateState();
		}

		public void Timeout()
		{
			model.PrepareState();
			model.HeaderLocKey = "Client_AutoMatch_Accept_TimeoutTitle";
			model.MessageLocKey = "Client_AutoMatch_Accept_TimeoutDescription";
			model.Timer = 3;
			model.Timout = Close;
			model.TimerReverse = true;
			UpdateState();
		}

		public void Return(Action onCompelted)
		{
			model.PrepareState();
			model.HeaderLocKey = "Client_AutoMatch_Accept_ReturnTitle";
			model.MessageLocKey = "Client_AutoMatch_Accept_ReturnDescription";
			model.Timer = 3;
			model.Timout = () => onCompelted?.Invoke();
			model.TimerReverse = true;
			UpdateState();
		}

		private void UpdateState()
		{
			if (!model.IsShowed)
			{
				model.IsShowed = true;
				uiService
					.Begin<AutoMatchAcceptWindow>()
					.WithInit(UpdateWindow)
					.Show();
				return;
			}

			UpdateWindow(uiService.Get<AutoMatchAcceptWindow>());
		}

		private void UpdateWindow(AutoMatchAcceptWindow window)
		{
			window.Clear();
			window.SetAcceptButtonVisible(model.AcceptVisible);
			window.SetDeclineButtonVisible(model.DeclineVisible);
			window.SetAcceptAction(value => OnAccepted?.Invoke(value));
			window.SetFillVisible(model.Timer.HasValue);
			window.SetHeaderText(gameDatabase.GetLocalization(model.HeaderLocKey));

			var messageVisible = !string.IsNullOrEmpty(model.MessageLocKey);
			window.SetMessgeVisible(messageVisible);
			if (messageVisible)
				window.SetMessageText(gameDatabase.GetLocalization(model.MessageLocKey));

			if (model.Timer.HasValue)
				TimoutAsync(window.SetFill, model.Timout, model.Timer.Value, model.TimerReverse, model.Token).Forget();
		}

		private static async UniTask TimoutAsync(Action<float> setter, Action onTimeout, int timer, bool reverse, CancellationToken token)
		{
			var current = 0f;
			while (current < timer)
			{
				setter?.Invoke(GetProgress());
				await UniTask.Yield(PlayerLoopTiming.Update, token);
				current += Time.deltaTime;
			}

			token.ThrowIfCancellationRequested();
			onTimeout?.Invoke();
			
			return;
			float GetProgress() => reverse ? 1 - Mathf.Clamp01(current / timer) : Mathf.Clamp01(current / timer);
		}
	}
}