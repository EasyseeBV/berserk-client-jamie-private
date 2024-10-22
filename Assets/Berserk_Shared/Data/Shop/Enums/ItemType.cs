using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Shop.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ItemType
	{
		Currency,
		Card,
		LootBox,
		Hero,
		Customization,
		Emotion,
		Consumable
	}
}