using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum AiBehaviourNodeType
	{
		Inverter,
		Selector,
		Sequence,
		Nothing,

		Random,
		DebugLog,
		NextTurn,

		TryPlayCreature,
		TryPlayPositiveCard,
		TryPlayNegativeCard,
		TryCreatureAttack,
		TryCreatureAttackOpponent,
		TryHeroAttackCreature,
		TryHeroAttackOpponent,

		CheckTurn,
		SelfCreaturesCount,
		OpponentCreaturesCount,
		WaitUserAction
	}
}