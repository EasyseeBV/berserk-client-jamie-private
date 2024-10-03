using Berserk.Shared.SignalR.Enums;

namespace Berserk.Shared.Data.Lobby.Leagues
{
	public class LeagueAutoMatchActionDeniedModel
	{
		public LobbyLeagueAction Action { get; set; }
		public string Reason { get; set; }
	}
}