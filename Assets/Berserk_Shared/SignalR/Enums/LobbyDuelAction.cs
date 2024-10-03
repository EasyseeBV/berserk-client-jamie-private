using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.SignalR.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum LobbyDuelAction
	{
		Created,
		Closed,
		PlayerJoined,
		PlayerLeft,
		Started
	}
}