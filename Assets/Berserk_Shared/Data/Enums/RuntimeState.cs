using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum RuntimeState
	{
		InDeck = 0,
		InHand,
		InTable,
		InDiscard,
		InChoose,
		InShowAll,
		InExile,
		InShow
	}
}