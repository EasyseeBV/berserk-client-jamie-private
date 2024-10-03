using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Rarity
	{
		Basic,
		Common,
		Rare,
		Mythic,
		Epic,
		Legendary,
	}
}