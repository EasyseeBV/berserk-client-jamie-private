using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum GameEndReason
	{
		None = 0,
		Afk,
		Disconnected,
		Surrender,
		Baned,
		Maintenance,
		MatchFailedToStart,
		MatchFailedToRecoverFromReddis,
		AbandonedTutorial,
		Technical,
		BothBots,
		MatchDodged,
		AbandonedSession
	}
}