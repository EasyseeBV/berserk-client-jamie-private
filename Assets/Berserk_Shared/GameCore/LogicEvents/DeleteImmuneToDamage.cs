using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class DeleteImmuneToDamage : LogicEvent
	{
		public DamageType DamageType { get; }
		public int EffectId { get; }
		public int RuntimeId { get; }

		[JsonConstructor]
		public DeleteImmuneToDamage(DamageType damageType, int runtimeId, int effectId)
		{
			DamageType = damageType;
			RuntimeId = runtimeId;
			EffectId = effectId;
		}

		public DeleteImmuneToDamage(DamageValue damageValue, int runtimeId)
		{
			DamageType = damageValue.DamageType;
			RuntimeId = runtimeId;
			EffectId = damageValue.EffectId;
		}
	}
}