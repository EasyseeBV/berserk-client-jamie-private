using System;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Lobby.Leagues;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Lobby.Network;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using RR.Core.Extensions;
using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Leagues
{
	public class LeagueSignalProcessor : ILeagueSignalProcessor, IInitializable, IDisposable
	{
		private readonly ILobbyHub lobbyHub;
		public event Action<LobbyLeagueAction, object> OnUpdateReceived;
		public event Action OnConnectedSuccess;
		public event Action OnReconnectedSuccess;
		public event Action OnConnectionLost;

		public LeagueSignalProcessor(ILobbyHub lobbyHub)
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
			if (!Enum.TryParse(target, out LobbyLeagueAction action))
				return;

			var jsonData = args.FirstOrDefault()?.ToString() ?? string.Empty;
			object deserialized = action switch
			{
				LobbyLeagueAction.LeagueMatchAccept 
					=> JsonConvert.DeserializeObject<LeagueAutoMatchStateModel>(jsonData),
				
				LobbyLeagueAction.LeagueJoinAutoMatch 
					=> JsonConvert.DeserializeObject<LeagueAutoMatchStateModel>(jsonData),
				
				LobbyLeagueAction.LeagueMatchReadyToAccept 
					=> JsonConvert.DeserializeObject<LeagueMatchReadyToAcceptModel>(jsonData),
				
				LobbyLeagueAction.LeagueAutoMatchState 
					=> JsonConvert.DeserializeObject<LeagueAutoMatchStateModel>(jsonData),
				
				LobbyLeagueAction.LeagueAutoMatchActionDenied 
					=> JsonConvert.DeserializeObject<LeagueAutoMatchActionDeniedModel>(jsonData),
				
				LobbyLeagueAction.LeagueOpponentDeclinedMatch 
					=> JsonConvert.DeserializeObject<LeagueAutoMatchStateModel>(jsonData),
				_ => null
			};
			
			OnUpdateReceived?.Invoke(action, deserialized);
		}
	}
}