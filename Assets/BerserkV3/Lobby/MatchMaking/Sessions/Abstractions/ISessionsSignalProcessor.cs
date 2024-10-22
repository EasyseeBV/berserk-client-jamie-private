using System;
using Berserk.Shared.Data.Lobby.Matchmaking;
using Berserk.Shared.SignalR.Enums;

namespace BerserkV3.Lobby.MatchMaking.Sessions
{
	public interface ISessionsSignalProcessor
	{
		event Action<LobbySessionsAction, ActiveSessionModel> OnUpdateReceived;
	}
}