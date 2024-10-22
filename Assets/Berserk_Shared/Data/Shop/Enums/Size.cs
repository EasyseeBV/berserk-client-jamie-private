using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Shop.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Size
	{
		Small,
		Average,
		Big
	}
}