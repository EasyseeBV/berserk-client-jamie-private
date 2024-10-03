using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Heal)]
	[EffectKeyword(EffectKeyword.Regen)]
	public class RestoreEffect : ChangeStatEffect
	{
		protected override void ChangeTargetStat(IntStat stat, IRuntimeGameObject target)
		{
			target.TakeRestore(ValueModRounded(stat), Executor);
		}

		protected override IEnumerable<IntStat> GetAffectedStats(IRuntimeGameObject target)
		{
			return new[] {target.RuntimeData.Hp};
		}
	}
}