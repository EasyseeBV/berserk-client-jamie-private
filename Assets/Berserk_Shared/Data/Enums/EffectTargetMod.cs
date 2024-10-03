using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectTargetMod
	{
		None,
		Self,
		PlayerPicked,
		EntityAttack,
		Random,
		NeighboursLeft,
		NeighboursRight,
		NeighboursBoth,
	}
}