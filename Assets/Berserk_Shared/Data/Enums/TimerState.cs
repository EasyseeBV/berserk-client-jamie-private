using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TimerState
	{
		NotStarted = 0,
		Mulligan = 2,
		Ready = 4,
		Game = 6,
		Ended = 16
	}
}