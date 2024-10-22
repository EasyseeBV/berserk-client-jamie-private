using System;
using Berserk.Shared.Data.Lobby.Matchmaking.Duels;
using Berserk.Shared.SignalR.Enums;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public interface IDuelsSignalProcessor
	{
		event Action<LobbyDuelAction, DuelRoomModel> OnUpdateReceived;
	}
}