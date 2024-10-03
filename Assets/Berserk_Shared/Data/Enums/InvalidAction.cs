using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum InvalidAction
	{
		None,
		NoSpellTarget,
		NoAvailableTargets,
		Immune,
		ImmuneToDamage,
		ImmuneToKeyword,
		DeckIsEmpty,
		ShouldTargetTaunt,
		CantTargetStealth,
		TableIsFull,
		EmotionsNotCustomised,
		ImNotAvailable
	}
}