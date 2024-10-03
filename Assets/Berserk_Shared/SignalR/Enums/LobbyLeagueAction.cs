using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.SignalR.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum LobbyLeagueAction
	{
		LeagueJoinAutoMatch,
		LeagueLeaveAutoMatch,
		LeagueMatchAccept,
		LeagueMatchReadyToAccept,
		LeagueOpponentDeclinedMatch,
		LeagueAutoMatchState,
		LeagueAutoMatchActionDenied,
		LeagueMatchStarting,
		LeagueLeaveAutoMatchDisconnected,
	}
}