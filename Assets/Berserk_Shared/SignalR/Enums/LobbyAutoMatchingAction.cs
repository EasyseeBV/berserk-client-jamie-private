using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.SignalR.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum LobbyAutoMatchingAction
	{
		AutoMatchJoin,
		AutoMatchLeave,
		AutoMatchAccept,
		AutoMatchReadyToAccept,
		AutoMatchOpponentDeclined,
		AutoMatchState,
		AutoMatchActionDenied,
		AutoMatchStarting,
		AutoMatchLeaveDisconnected,
	}
}