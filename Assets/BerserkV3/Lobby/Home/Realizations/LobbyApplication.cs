using System;
using System.Linq;
using BerserkV3.Common.Abstractions;
using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Common.Network;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.UIService;
using BerserkV3.Common.Utils;
using BerserkV3.Generic.UndoSystem;
using BerserkV3.Lobby.Home.Abstractions;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.Core.DebugSystem;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.UIService;
using Statistics;
using Zenject;

namespace BerserkV3.Lobby.Home
{
	/// <summary>
	/// This is a bootstrap class, setup the lobby
	/// </summary>
	public class LobbyApplication : IDisposable, IInitializable
	{
		private readonly IUIService uiService;
		private readonly IUndoSystem undoSystem;
		private readonly ISceneService sceneService;
		private readonly ITutorialApplication tutorialApplication;
		private readonly IStatisticApplication statisticApplication;
		private readonly IRedirectionApplication redirectionApplication;
		private readonly ILobbyNetworkApplication networkApplication;
		private readonly IMainMenuApplication mainMenuApplication;
		
		public LobbyApplication(
			IUIService uiService,
			IUndoSystem undoSystem, 
			ISceneService sceneService,
			ITutorialApplication tutorialApplication,
			IStatisticApplication statisticApplication,
			IRedirectionApplication redirectionApplication,
			ILobbyNetworkApplication networkApplication,
			IMainMenuApplication mainMenuApplication)
		{
			this.uiService = uiService;
			this.undoSystem = undoSystem;
			this.sceneService = sceneService;
			this.tutorialApplication = tutorialApplication;
			this.statisticApplication = statisticApplication;
			this.redirectionApplication = redirectionApplication;
			this.networkApplication = networkApplication;
			this.mainMenuApplication = mainMenuApplication;
		}

		public void Dispose()
		{
			undoSystem.Clear(); // TODO rework
			LobbyBus.LogOut.Unsubscribe(RedirectToAuth); // TODO rework
			LobbyBus.LeaguesRefereshRequered.Unsubscribe(OnLeagueRefreshRequested); // TODO rework
			
			uiService.DestroyAll(UILayer.LobbyUIGroup);
		}

		public async void Initialize()
		{
			try
			{
				LobbyBus.LogOut.SubscribeRaw(RedirectToAuth); // TODO rework
				LobbyBus.LeaguesRefereshRequered.SubscribeRaw(OnLeagueRefreshRequested); // TODO rework
				AnalyticsBus.SendDelayedEvents.Publish(); // TODO rework
				await UniTask.WhenAll(LoadLeaguesAsync(), networkApplication.InitAsync(), statisticApplication.Init().AsUniTask(), tutorialApplication.InitAsync().AsUniTask()).AddLoadingTask();
				
				mainMenuApplication.Init();
				await redirectionApplication.RedirectAsync();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void OnLeagueRefreshRequested()  // TODO rework
		{
			LoadLeaguesAsync().Forget(e => RRLogger.Error(e));
		}
		
		private async UniTask LoadLeaguesAsync()  // TODO rework
		{
			var leaguesResponse = await LobbyAPI.GetAvailableLeagues();
			if (!leaguesResponse || leaguesResponse.Data == null)
			{
				RRLogger.Error($"Error loading leagues : {leaguesResponse.GetMessage()}");
				LobbyBus.Leagues.Repeat();
				return;
			}
			
			LobbyBus.Leagues += leaguesResponse.Data.OrderByDescending(l => l.LeagueName).ToList();
		}

		private async void RedirectToAuth()  // TODO rework
		{
			await User.LogOutAsync().AddLoadingTask();
			await sceneService.LoadAsync(Scene.StartUp).AddLoadingTask();
		}
	}
}