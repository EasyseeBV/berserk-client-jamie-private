using System;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.SignalR.Enums;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public interface IDuelsSignalProcessor
	{
		event Action<LobbyDuelAction, LobbyDuelRoomModel> OnUpdateReceived;
	}
}