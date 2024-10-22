using System;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Lobby.Network;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Zenject;

namespace BerserkV3.Lobby.MatchMaking.AutoMatching
{
	public class AutoMatchSignalProcessor : IAutoMatchSignalProcessor, IInitializable, IDisposable
	{
		private readonly ILobbyHub lobbyHub;
		public event Action<LobbyAutoMatchingAction, AutoMatchBaseModel> OnUpdateReceived;
		public event Action OnConnectedSuccess;
		public event Action OnReconnectedSuccess;
		public event Action OnConnectionLost;

		public AutoMatchSignalProcessor(ILobbyHub lobbyHub)
		{
			this.lobbyHub = lobbyHub;
		}

		public UniTask SendAsync(object target, params object[] args)
		{
			return lobbyHub.SendAsync(target?.ToString(), args).AsUniTask();
		}

		public UniTask SendAsync(object target, CancellationToken token = default, params object[] args)
		{
			return lobbyHub.SendAsync(target?.ToString(), token, args).AsUniTask();
		}

		public void Initialize()
		{
			lobbyHub.OnMessageReceived += OnMessageReceived;
			lobbyHub.OnConnectedSuccess += OnConnectedSuccess;
			lobbyHub.OnReconnectedSuccess += OnReconnectedSuccess;
			lobbyHub.OnConnectionClosed += OnConnectionLost;
		}

		public void Dispose()
		{
			lobbyHub.OnMessageReceived -= OnMessageReceived;
			lobbyHub.OnConnectedSuccess -= OnConnectedSuccess;
			lobbyHub.OnReconnectedSuccess -= OnReconnectedSuccess;
			lobbyHub.OnConnectionClosed -= OnConnectionLost;
			OnReconnectedSuccess = null;
			OnConnectedSuccess = null;
			OnConnectionLost = null;
			OnUpdateReceived = null;
		}

		private void OnMessageReceived(string target, object[] args)
		{
			if (!Enum.TryParse(target, out LobbyAutoMatchingAction action))
				return;

			var jsonData = args.FirstOrDefault()?.ToString() ?? string.Empty;
			AutoMatchBaseModel deserialized = action switch
			{
				LobbyAutoMatchingAction.AutoMatchJoin 
				or LobbyAutoMatchingAction.AutoMatchLeave 
				or LobbyAutoMatchingAction.AutoMatchAccept 
				or LobbyAutoMatchingAction.AutoMatchReadyToAccept 
				or LobbyAutoMatchingAction.AutoMatchOpponentDeclined 
				or LobbyAutoMatchingAction.AutoMatchState 
					=> JsonConvert.DeserializeObject<AutoMatchStateModel>(jsonData),
				
				LobbyAutoMatchingAction.AutoMatchActionDenied 
					=> JsonConvert.DeserializeObject<AutoMatchActionDeniedModel>(jsonData),
				
				_ => null
			};
			
			OnUpdateReceived?.Invoke(action, deserialized);
		}
	}
}