using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{
	[EffectKeyword(EffectKeyword.BuffReplaceHealth)]
	public class BuffStatReplaceHealthEffect : BuffStatReplaceEffect
	{
		protected override IEnumerable<IntStat> GetAffectedStats(IRuntimeGameObject target)
		{
			yield return target.RuntimeData.Hp;
		}

		protected override int? GetRestrictionMinCurrent(IntStat stat)
		{
			return 1;
		}

		protected override int? GetRestrictionMinMax(IntStat stat)
		{
			return 1;
		}
	}
}