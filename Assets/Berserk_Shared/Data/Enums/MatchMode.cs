using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum MatchMode
	{
		None = 0,
		Duel,
		Ranked,
		Practice,
		Campaign,
		Tutorial,
		AutoTest
	}
}
