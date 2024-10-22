using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.SignalR.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum SignalType
	{
		None = 0,
		Ping,
		LobbyUpdate,
		GameAction,
		GameVisual,
		Direct,
		LogicEvents,
		Information,
		
		TimeOut,
		Error,
		DropConnection,
		Reauthorization,
		RefreshToken,
		Matched,
		VulcaniteRentExpired,
		SubscriptionExpired,
		RankedDecay,
		Store
	}
}