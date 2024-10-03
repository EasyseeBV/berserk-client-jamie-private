using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{
	[EffectKeyword(EffectKeyword.BuffReplaceMana)]
	public class BuffStatReplaceManaEffect : BuffStatReplaceEffect
	{
		protected override IEnumerable<IntStat> GetAffectedStats(IRuntimeGameObject target)
		{
			yield return target.RuntimeData.Mana;
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