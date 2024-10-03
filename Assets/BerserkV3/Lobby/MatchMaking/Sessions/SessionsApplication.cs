using System;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Common.ProgressDrawer;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using RR.Game.TutorialSystemV2.Realizations;
using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Sessions
{
	public class SessionsApplication : ISessionsApplication, IInitializable, IDisposable
	{
		private readonly ISharedConfig sharedConfig;
		private readonly ISceneService sceneService;
		private readonly IProgressDrawer progressDrawer;
		private readonly ISessionsSignalProcessor sessionsSignalProcessor;
		private readonly IBerserkTutorialApplication tutorialApplication;

		public SessionsApplication(
			ISharedConfig sharedConfig,
			ISceneService sceneService,
			IProgressDrawer progressDrawer,
			ISessionsSignalProcessor sessionsSignalProcessor,
			IBerserkTutorialApplication tutorialApplication)
		{
			this.sharedConfig = sharedConfig;
			this.sceneService = sceneService;
			this.progressDrawer = progressDrawer;
			this.sessionsSignalProcessor = sessionsSignalProcessor;
			this.tutorialApplication = tutorialApplication;
		}

		public void Initialize()
		{
			sessionsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
			sessionsSignalProcessor.OnUpdateReceived += OnMessageReceived;
		}

		public void Dispose()
		{
			sessionsSignalProcessor.OnUpdateReceived -= OnMessageReceived;
		}

		public async UniTask<bool> TryConnectGameAsync()
		{
			var roomResponse = await LobbyAPI.GetActiveSession();
			return await ConnectGameAsync(roomResponse.Data);
		}

		public async UniTask<bool> ConnectGameAsync(ActiveSessionModel model)
		{
			if (model?.Players == null || model.Players.All(x => x.UserId != User.Id))
				return false;
			
			await progressDrawer.SetProgressTypeAsync(ProgressType.Versus);
			progressDrawer.CreateProgress()
				.ProgressTickAsync(sharedConfig.GameLoadingPreviewTime, 10)
				.Forget();
			
			await progressDrawer.ShowAsync(model.Players.ToArray<object>());
			progressDrawer.AddProgress(sceneService.Load(Scene.Game));
			return true;
		}

		private void OnMessageReceived(LobbySessionsAction action, ActiveSessionModel activeSessionModel)
		{
			if (action != LobbySessionsAction.ActiveSession 
			    || activeSessionModel.Players.All(x => x.UserId != User.Id))
				return;

			if (activeSessionModel.MatchMode != MatchMode.Tutorial)
				tutorialApplication.SkipAsync().Forget();
			
			ConnectGameAsync(activeSessionModel).Forget();
		}
	}
}