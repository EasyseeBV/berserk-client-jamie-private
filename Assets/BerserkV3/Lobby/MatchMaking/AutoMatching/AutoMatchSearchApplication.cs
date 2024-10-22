using System;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Lobby.UI.Home.General;
using Cysharp.Threading.Tasks;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Lobby.MatchMaking.AutoMatching
{
	public class AutoMatchSearchApplication : IAutoMatchSearchApplication, IDisposable
	{
		private readonly IUIService uiService;
		private readonly IGameDatabase gameDatabase;
		private CancellationTokenSource timerSource;
		private int? serachingTimerMem;
		
		public event Action OnCancel;

		public AutoMatchSearchApplication(
			IUIService uiService,
			IGameDatabase gameDatabase)
		{
			this.uiService = uiService;
			this.gameDatabase = gameDatabase;
		}

		public void Searching()
		{
			timerSource?.Cancel();
			timerSource?.Dispose();
			timerSource = new CancellationTokenSource();
			var searchingWidget = uiService.Get<GeneralWindow>().SearchingWidget;
			searchingWidget.Clear();
			searchingWidget.SetActive(true);
			searchingWidget.SetActiveTimer(true);
			searchingWidget.SetText(gameDatabase.GetLocalization("Client_AutoMatch_Search_Title"));
			searchingWidget.SetCancelAction(() => OnCancel?.Invoke());

			var timerFormat = gameDatabase.GetLocalization("Client_AutoMatch_Search_TimerFormat");
			var startFrom = serachingTimerMem ?? 0;
			UpdateTimerAsync(startFrom, UpdateSerchTimer, timerSource.Token).Forget();
			
			return;
			void UpdateSerchTimer(int value)
			{
				if (!searchingWidget)
					return;
				
				searchingWidget.SetTimerText(string.Format(timerFormat, value));
			}
		}

		public void Accepting()
		{
			timerSource?.Cancel();
			timerSource?.Dispose();
			timerSource = null;
			
			var searchingWidget = uiService.Get<GeneralWindow>().SearchingWidget;
			searchingWidget.Clear();
			searchingWidget.SetActive(true);
			searchingWidget.SetText(gameDatabase.GetLocalization("Client_AutoMatch_Search_AcceptionTitle"));
			searchingWidget.SetCancelAction(() => OnCancel?.Invoke());
		}

		public void Leaving()
		{
			timerSource?.Cancel();
			timerSource?.Dispose();
			timerSource = null;
			
			var searchingWidget = uiService.Get<GeneralWindow>().SearchingWidget;
			searchingWidget.Clear();
			searchingWidget.SetActive(true);
			searchingWidget.SetText(gameDatabase.GetLocalization("Client_AutoMatch_Search_LeavingTitle"));
		}

		public void Stop()
		{
			timerSource?.Cancel();
			timerSource?.Dispose();
			timerSource = null;
			var searchingWidget = uiService.Get<GeneralWindow>().SearchingWidget;
			searchingWidget.SetActive(false);
			searchingWidget.Clear();
			serachingTimerMem = null;
		}


		private async UniTask UpdateTimerAsync(int startFrom, Action<int> setter, CancellationToken token)
		{
			serachingTimerMem = startFrom;
			while (!token.IsCancellationRequested && Application.isPlaying)
			{
				setter?.Invoke(serachingTimerMem.Value);
				await UniTask.Delay(1000, true, cancellationToken: token);
				serachingTimerMem += 1;
			}
		}

		public void Dispose()
		{
			OnCancel = null;
		}
	}
}