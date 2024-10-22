using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{
	[EffectKeyword(EffectKeyword.BuffArmor)]
	public class BuffArmor : BuffStatBaseEffect
	{
		protected override bool RevertCurrentWhenExpire => true;
		
		protected override IEnumerable<IntStat> GetAffectedStats(IRuntimeGameObject target)
		{
			yield return target.RuntimeData.Armor;
		}
	}
}