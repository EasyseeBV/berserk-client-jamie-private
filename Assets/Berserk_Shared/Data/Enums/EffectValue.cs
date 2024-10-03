using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectValue
	{
		Integer,
		PercentFromCurrent,
		PercentFromMaximum,
		AlliedCount,
		AlliedCountExceptSelf,
		AlliedHandCount,
		TableCardsCount,
		FreeSelfTableSpace,
		FreeOpponentTableSpace,
		EnemyCount,
		TargetCount,
		TurnWithoutCards,
		SelfLava,
		OpponentLava
	}
}