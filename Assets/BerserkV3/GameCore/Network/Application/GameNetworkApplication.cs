using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.AppTime;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.UI;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.Utils;
using BestHTTP;
using Cysharp.Threading.Tasks;
using GameCore;
using RR.Core.DebugSystem;
using RR.Game.TutorialSystemV2.Realizations;
using UI;
using Zenject;

namespace BerserkV3.GameCore.Network
{
	public class GameNetworkApplication : DisposableWithCts, IInitializable
	{
		private readonly IGameHub gameHub;
		private readonly IGameContext gameContext;
		private readonly ISceneService sceneService;
		private readonly IReconnectionView reconnectionView;
		private readonly IBerserkTutorialApplication tutorialApplication;
		private readonly IAppTimeSynchronizer appTimeSynchronizer;
		private readonly ConnectionHandler connectionHandler;

		public GameNetworkApplication(
			IGameHub gameHub, 
			IGameContext gameContext,
			ISceneService sceneService,
			IInstantiator instantiator,
			IReconnectionView reconnectionView,
			IBerserkTutorialApplication tutorialApplication,
			IAppTimeSynchronizer appTimeSynchronizer)
		{
			this.gameHub = gameHub;
			this.gameContext = gameContext;
			this.sceneService = sceneService;
			this.reconnectionView = reconnectionView;
			this.tutorialApplication = tutorialApplication;
			this.appTimeSynchronizer = appTimeSynchronizer;
			connectionHandler = instantiator.Instantiate<ConnectionHandler>();
		}

		public void Initialize()
		{
			HTTPManager.Setup();

			connectionHandler.OnFailedConnection += HandleConnectionLoose;
			gameHub.OnConnectedSuccess += reconnectionView.Close;
			gameHub.OnConnectedSuccess += RequestInitializeGame;
			gameHub.OnReconnectingAttempt += HandleReconnectAttempt;
			gameHub.OnRestartRequired += HandleConnectionLoose;
			GameCoreBus.OnReconnectRequired.SubscribeRaw(HandleConnectionLoose);
			GameCoreBus.OnReauthorizationRequired.SubscribeRaw(HandleReauthorization);
			gameHub.ConnectByAcessTokenAsync().Forget();
			connectionHandler.Initialize();
			TaskUtil.RetryLoopAsync(appTimeSynchronizer.SynchronizeTimeAsync, Token).Forget(e => RRLogger.Error(e));
		}

		public override void Dispose()
		{
			if (IsDisposed)
				return;
			
			base.Dispose();
			GameCoreBus.OnReconnectRequired.Unsubscribe(HandleConnectionLoose);
			GameCoreBus.OnReauthorizationRequired.Unsubscribe(HandleReauthorization);
			gameHub.OnConnectedSuccess -= reconnectionView.Close;
			gameHub.OnConnectedSuccess -= RequestInitializeGame;
			gameHub.OnReconnectingAttempt -= HandleReconnectAttempt;
			gameHub.OnRestartRequired -= HandleConnectionLoose;
			gameHub.Close();
			reconnectionView.Close();
			connectionHandler.Dispose();
			GameCoreBus.Reset();
		}

		private void RequestInitializeGame()
		{
			gameHub.ReadyToInitializeAsync().Forget(DefaultSharedLogger.Error);
		}

		private void HandleConnectionLoose()
		{
			HandleConnectionLoose("There was a connection loss, please retry the connection, press button to continue.");
		}
		
		private void HandleReauthorization(string message)
		{
			if (IsDisposed)
				return;

			User.LogOutAsync()
				.ContinueWith(() => HandleConnectionLoose(message))
				.Forget(_ => HandleConnectionLoose(message));
		}
		
		private void HandleConnectionLoose(string message)
		{
			if (IsDisposed)
				return;
			
			Dispose();
			ConfirmationDialog.Instance.Init()
				.SetMessage(message)
				.SetTitle("Connection")
				.SetOk("OK")
				.SetCancel()
				.SetAnyResponse(HandleGameLeft)
				.Apply();
		}

		private void HandleReconnectAttempt()
		{
			if (gameContext.RuntimeData is {IsEnded: true} or {MatchMode: MatchMode.Tutorial})
			{
				HandleConnectionLoose("There was a connection loss, please start the tutorial game again, press button to continue.");
				return;
			}

			reconnectionView.Show();
		}
		
		private async void HandleGameLeft()
		{
			if (gameContext.RuntimeData is {MatchMode: MatchMode.Tutorial})
				await tutorialApplication.ResetAsync().AddLoadingTask();
			
			if (User.IsAuthorized)
				User.AddRedirection(new AuthRedirectionArg(nameof(AuthCompleteState)));
			await sceneService.LoadAsync(Scene.StartUp).AddLoadingTask();
		}
	}
}