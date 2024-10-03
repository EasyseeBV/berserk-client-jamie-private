using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{

	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectPhaseTrigger
	{
		AnySide,
		AlliedSide,
		EnemySide,
		TurnSide,
		ExceptSelf,
		OnlySelf,
		BuildingType,
		CreatureType,
		SpellType,
		HeroType,
	}

}