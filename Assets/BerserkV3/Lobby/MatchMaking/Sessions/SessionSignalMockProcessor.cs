using System;
using Berserk.Shared.Data.Lobby.Matchmaking;
using Berserk.Shared.SignalR.Enums;

namespace BerserkV3.Lobby.MatchMaking.Sessions
{
	public class SessionSignalMockProcessor : ISessionsSignalProcessor, IDisposable
	{
		public event Action<LobbySessionsAction, ActiveSessionModel> OnUpdateReceived;
		public void Dispose()
		{
			OnUpdateReceived = null;
		}
	}
}