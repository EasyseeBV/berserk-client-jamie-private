using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ExpirePhase
	{
		None,
		BeforeRoundStarted,
		AfterRoundEnded,
		BeforeEachRound,
		AfterEachRound,
		Execute
	}
}