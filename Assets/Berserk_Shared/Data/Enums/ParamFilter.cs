using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ParamFilter
	{
		None,
		ManaHighest,
		ManaSmallest,
		
		AttackHighest,
		AttackSmallest,
		
		HpHighest,
		HpSmallest,
		HpDamaged,
		
		ArmorHighest,
		ArmorSmallest,
		ArmorDamaged,
		
		ManaEqual,
		ManaEqualExecutor,
		ManaLess,
		ManaLessExecutor,
		ManaLessOrEqual,
		ManaLessOrEqualExecutor,
		ManaMore,
		ManaMoreExecutor,
		ManaMoreOrEqual,
		ManaMoreOrEqualExecutor,
		
		AttackEqual,
		AttackEqualExecutor,
		AttackLess,
		AttackLessExecutor,
		AttackLessOrEqual,
		AttackLessOrEqualExecutor,
		AttackMore,
		AttackMoreExecutor,
		AttackMoreOrEqual,
		AttackMoreOrEqualExecutor,

		HpEqual,
		HpEqualExecutor,
		HpLess,
		HpLessExecutor,
		HpLessOrEqual,
		HpLessOrEqualExecutor,
		HpMore,
		HpMoreExecutor,
		HpMoreOrEqual,
		HpMoreOrEqualExecutor,
		
		ArmorEqual,
		ArmorEqualExecutor,
		ArmorLess,
		ArmorLessExecutor,
		ArmorLessOrEqual,
		ArmorLessOrEqualExecutor,
		ArmorMore,
		ArmorMoreExecutor,
		ArmorMoreOrEqual,
		ArmorMoreOrEqualExecutor
	}
}