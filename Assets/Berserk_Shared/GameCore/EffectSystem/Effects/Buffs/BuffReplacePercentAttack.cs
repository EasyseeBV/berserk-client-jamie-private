using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{
	[EffectKeyword(EffectKeyword.BuffReplaceAttackPercent)]
	public class BuffReplacePercentAttack : BuffReplacePercentEffect
	{
		protected override bool RevertCurrentWhenExpire => true;
		
		protected override IEnumerable<IntStat> GetAffectedStats(IRuntimeGameObject target)
		{
			yield return target.RuntimeData.Attack;
		}
		
		protected override int? GetRestrictionMinCurrent(IntStat stat)
		{
			return 0;
		}

		protected override int? GetRestrictionMinMax(IntStat stat)
		{
			return 0;
		}
	}
}