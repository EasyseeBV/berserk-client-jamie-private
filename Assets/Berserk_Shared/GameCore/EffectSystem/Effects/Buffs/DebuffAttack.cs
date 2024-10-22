using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{
	[EffectKeyword(EffectKeyword.DeBuffAttack)]
	public class DebuffAttack : BuffStatBaseEffect
	{
		protected override bool RevertCurrentWhenExpire => true;

		protected override IEnumerable<IntStat> GetAffectedStats(IRuntimeGameObject target)
		{
			yield return target.RuntimeData.Attack;
		}

		protected override int GetBuffCurrentValue(IntStat stat)
		{
			var valueMod = ValueModRounded(stat);
			var valueSign = Math.Sign(valueMod);
			var absValue = Math.Abs(valueMod);
			var maxStatValueRaw = Math.Abs(stat) - GetRestrictionMinCurrent(stat) ?? 0;
			var maxStatValueAvailable = Math.Max(0, maxStatValueRaw); // max value available to debuff
			return Math.Clamp(absValue, 0, maxStatValueAvailable) * valueSign; // clamp exist debuff value to available maximum value or zero, then restore it's sign.
		}

		protected override int GetBuffMaximumValue(IntStat stat)
		{
			var valueMod = ValueModRounded(stat);
			var valueSign = Math.Sign(valueMod);
			var absValue = Math.Abs(valueMod);
			var maxStatValueRaw = Math.Abs(stat.TotalMax) - GetRestrictionMinMax(stat) ?? 0;
			var maxStatValueAvailable = Math.Max(0, maxStatValueRaw);  // max value available to debuff
			return Math.Clamp(absValue, 0, maxStatValueAvailable) * valueSign;  // clamp exist debuff value to available maximum value or zero, then restore it's sign.
		}

		protected override int? GetRestrictionMinCurrent(IntStat stat)
		{
			return 0;
		}
	}
}