using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectExecutionOrder
	{
		First = -100,
		Default = 0,
		Last = 100
	}
}