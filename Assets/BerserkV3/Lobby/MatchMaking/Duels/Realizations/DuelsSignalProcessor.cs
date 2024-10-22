using System;
using System.Linq;
using Berserk.Shared.Data.Lobby.Matchmaking.Duels;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Lobby.Network;
using Newtonsoft.Json;
using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelsSignalProcessor : IDuelsSignalProcessor, IInitializable, IDisposable
	{
		private readonly ILobbyHub lobbyHub;
		public event Action<LobbyDuelAction, DuelRoomModel> OnUpdateReceived; 
		
		public DuelsSignalProcessor(ILobbyHub lobbyHub)
		{
			this.lobbyHub = lobbyHub;
		}

		public void Initialize()
		{
			lobbyHub.OnMessageReceived -= OnMessageReceived;
			lobbyHub.OnMessageReceived += OnMessageReceived;
		}

		public void Dispose()
		{
			lobbyHub.OnMessageReceived -= OnMessageReceived;
		}

		private void OnMessageReceived(string target, object[] args)
		{
			if (!Enum.TryParse(target, out LobbyDuelAction action))
				return;

			var jsonData = args.FirstOrDefault()?.ToString() ?? string.Empty;
			var duelRoomModel = JsonConvert.DeserializeObject<DuelRoomModel>(jsonData);
			OnUpdateReceived?.Invoke(action, duelRoomModel);
		}
	}
}