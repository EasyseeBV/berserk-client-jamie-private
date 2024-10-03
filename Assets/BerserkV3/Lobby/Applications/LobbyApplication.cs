using System;
using System.Linq;
using BerserkV3.Common.Abstractions;
using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Common.Network;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.Utils;
using BerserkV3.Generic.UndoSystem;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.Network;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.Core.DebugSystem;
using UI;
using Zenject;

namespace BerserkV3.Lobby.Applications
{
	public class LobbyApplication : IDisposable, IInitializable
	{
		private readonly IUndoSystem undoSystem;
		private readonly ISceneService sceneService;
		private readonly IRedirectionApplication redirectionApplication;
		private readonly ILobbyNetworkApplication networkApplication;
		private IDisposable tutorialHandlers;

		public LobbyApplication(
			IUndoSystem undoSystem, 
			ISceneService sceneService,
			IRedirectionApplication redirectionApplication,
			ILobbyNetworkApplication networkApplication)
		{
			this.undoSystem = undoSystem;
			this.sceneService = sceneService;
			this.redirectionApplication = redirectionApplication;
			this.networkApplication = networkApplication;
		}

		public void Dispose()
		{
			undoSystem.Clear();
			tutorialHandlers?.Dispose();
			tutorialHandlers = null;
			LobbyBus.LogOut.Unsubscribe(RedirectToAuth);
			LobbyBus.LeaguesRefereshRequered.Unsubscribe(OnLeagueRefreshRequested);
		}

		public async void Initialize()
		{
			try
			{
				LobbyBus.LogOut.SubscribeRaw(RedirectToAuth);
				LobbyBus.LeaguesRefereshRequered.SubscribeRaw(OnLeagueRefreshRequested);
				await UniTask.WhenAll(LoadLeaguesAsync(), LoadReportThanks(), networkApplication.InitAsync()).AddLoadingTask();
				
				AnalyticsBus.SendDelayedEvents.Publish();
				await redirectionApplication.RedirectAsync(); // can be redirected, track by bool result
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void OnLeagueRefreshRequested()
		{
			LoadLeaguesAsync().Forget(e => RRLogger.Error(e));
		}
		
		private async UniTask LoadLeaguesAsync()
		{
			var leaguesRespnse = await LobbyAPI.GetAvailableLeagues();
			if (!leaguesRespnse || leaguesRespnse.Data == null)
			{
				RRLogger.Error($"Error loading leagues : {leaguesRespnse.GetMessage()}");
				LobbyBus.Leagues.Repeat();
				return;
			}
			
			LobbyBus.Leagues += leaguesRespnse.Data.OrderByDescending(l => l.LeagueName).ToList();
		}

		private async void RedirectToAuth()
		{
			await User.LogOutAsync().AddLoadingTask();
			await sceneService.LoadAsync(Scene.StartUp).AddLoadingTask();
		}

		private async UniTask LoadReportThanks()
		{
			var reportThanksResponse = await IdentityAPI.GetReportThanks().AsUniTask();

			if (!reportThanksResponse.IsSuccess)
				return;

			var reportThanksModel = reportThanksResponse.Data;
			if (reportThanksModel is {ReportedPlayerIsBanned: true})
				ConfirmationDialog.Instance.Init()
					.SetMessage(reportThanksModel.Message)
					.SetTitle("")
					.SetCancel()
					.Apply();
		}
	}
}