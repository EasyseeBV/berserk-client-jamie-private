using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class AddImmuneToDamage : LogicEvent
	{
		public DamageType DamageType { get; }
		public EffectValue ValueMod { get; }
		public int Value { get; }
		public string TypeName { get; }
		public int RuntimeId { get; }
		public int EffectId { get; }

		[JsonConstructor]
		public AddImmuneToDamage(
			DamageType damageType,
			int runtimeId,
			string typeName,
			int value,
			EffectValue valueMod,
			int effectId)
		{
			DamageType = damageType;
			RuntimeId = runtimeId;
			TypeName = typeName;
			Value = value;
			ValueMod = valueMod;
			EffectId = effectId;
		}

		public AddImmuneToDamage(DamageValue damageType, int runtimeId)
		{
			DamageType = damageType.DamageType;
			RuntimeId = runtimeId;
			TypeName = damageType.GetType().Name;
			Value = damageType.Value;
			ValueMod = damageType.ValueMod;
			EffectId = damageType.EffectId;
		}
	}
}