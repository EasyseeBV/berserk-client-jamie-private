using Berserk.Shared.Data.Abstraction;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.UI;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Lobby;
using UI;
using Scene = BerserkV3.Common.SceneService.Scene;

namespace BerserkV3.Lobby.Applications
{
	public interface ILobbyNetworkApplication
	{
		UniTask InitAsync();
	}

	public class LobbyNetworkApplication : DisposableWithCts, ILobbyNetworkApplication
	{
		private bool isReconnectRequired;
		private readonly ILobbyHub lobbyHub;
		private readonly IReconnectionView reconnectionView;
		private readonly ISceneService sceneService;
		private readonly IGameDatabase gameDatabase;
		
		public LobbyNetworkApplication(
			ILobbyHub lobbyHub, 
			IReconnectionView reconnectionView,
			ISceneService sceneService,
			IGameDatabase gameDatabase)
		{
			this.lobbyHub = lobbyHub;
			this.reconnectionView = reconnectionView;
			this.sceneService = sceneService;
			this.gameDatabase = gameDatabase;
		}
		
		public UniTask InitAsync()
		{
			LobbyBus.OnReconnectRequired.SubscribeRaw(HandleConnectionLost);
			LobbyBus.OnReauthorizationRequired.SubscribeRaw(HandleReauthorization);
			lobbyHub.OnConnectedSuccess += reconnectionView.Close;
			lobbyHub.OnReconnectingAttempt += reconnectionView.Show;
			lobbyHub.OnRestartRequired += reconnectionView.Close;
			lobbyHub.OnRestartRequired += HandleConnectionLost;
			return lobbyHub.ConnectByAcessTokenAsync().AsUniTask();
		}
		
		public override void Dispose()
		{
			base.Dispose();
			if (IsDisposed)
				return;
			
			LobbyBus.OnReconnectRequired.Unsubscribe(HandleConnectionLost);
			LobbyBus.OnReauthorizationRequired.Unsubscribe(HandleReauthorization);
			lobbyHub.OnConnectedSuccess -= reconnectionView.Close;
			lobbyHub.OnReconnectingAttempt -= reconnectionView.Show;
			lobbyHub.OnRestartRequired -= reconnectionView.Close;
			lobbyHub.OnRestartRequired -= HandleConnectionLost;
			lobbyHub.Close();
		}
		
		private void HandleConnectionLost()
		{
			HandleConnectionLost(gameDatabase.GetLocalization("NetworkLoose"));
		}		
		
		private void HandleReauthorization(string message)
		{
			if (IsDisposed)
				return;

			User.LogOutAsync()
				.ContinueWith(() => HandleConnectionLost(message))
				.Forget(_ => HandleConnectionLost(message));
		}

		private void HandleConnectionLost(string message)
		{
			if (isReconnectRequired)
				return;
		
			isReconnectRequired = true;
			ConfirmationDialog.Instance.Init()
				.SetMessage(message)
				.SetTitle("Connection")
				.SetResponseOk(HandleLobbyLeft)
				.SetCancel()
				.Apply();
		}
		
		private async void HandleLobbyLeft()
		{
			if (User.IsAuthorized)
				User.AddRedirection(new AuthRedirectionArg(nameof(AuthCompleteState)));
			
			await sceneService.LoadAsync(Scene.StartUp).AddLoadingTask();
		}
	}
}
