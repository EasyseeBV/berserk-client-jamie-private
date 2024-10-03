using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.ChangeStat)]
	public class ChangeStatEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				foreach (var stat in GetAffectedStats(target))
					ChangeTargetStat(stat, target);
			}
		}

		protected virtual void ChangeTargetStat(IntStat stat, IRuntimeGameObject target)
		{
			stat.Add(ValueModRounded(stat));
		}
		
		protected virtual IEnumerable<IntStat> GetAffectedStats(IRuntimeGameObject target)
		{
			return EffectData.GetStatsFromMeta(target);
		}
	}
}