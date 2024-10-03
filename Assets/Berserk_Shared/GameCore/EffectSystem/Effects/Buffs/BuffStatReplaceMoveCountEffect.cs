using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{

	[EffectKeyword(EffectKeyword.Sleeping)]
	public class BuffStatReplaceMoveCountEffect : BuffStatReplaceEffect
	{
		protected override IEnumerable<IntStat> GetAffectedStats(IRuntimeGameObject target)
		{
			return new[] {target.RuntimeData.MoveCount};
		}
	}

}