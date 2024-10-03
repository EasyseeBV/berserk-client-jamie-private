using System;
using System.Linq;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.SignalR.Enums;
using BerserkV3.Lobby.Network;
using Newtonsoft.Json;
using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Sessions
{
	public class SessionsSignalProcessor : IInitializable, IDisposable, ISessionsSignalProcessor
	{
		private readonly ILobbyHub lobbyHub;
		public event Action<LobbySessionsAction, ActiveSessionModel> OnUpdateReceived; 
		
		public SessionsSignalProcessor(ILobbyHub lobbyHub)
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
			OnUpdateReceived = null;
			lobbyHub.OnMessageReceived -= OnMessageReceived;
		}		
		
		private void OnMessageReceived(string target, object[] args)
		{
			if (!Enum.TryParse(target, out LobbySessionsAction action))
				return;

			var jsonData = args.FirstOrDefault()?.ToString() ?? string.Empty;
			var duelRoomModel = JsonConvert.DeserializeObject<ActiveSessionModel>(jsonData);
			OnUpdateReceived?.Invoke(action, duelRoomModel);
		}
	}
}