using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectExecuteTarget
	{
		All = -1,
		Targets = 0,
		Executor = 2,
		Opposite = 4,
		Self = 8,
		TurnOwner = 16,
	}

}