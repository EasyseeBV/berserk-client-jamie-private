using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.RuntimeObjects
{
	public class RuntimeHero : RuntimeGameObject, IRuntimeHero
	{
		public new IRuntimeHeroData RuntimeData => (IRuntimeHeroData)base.RuntimeData;
		public new IHeroData Data => (IHeroData)base.Data;
		public override bool IsDead => RuntimeData.Hp <= 0;

		public override void ResetBuffStats()
		{
			base.ResetBuffStats();
			ResetStat(RuntimeData.AbilityMoveCount, RuntimeData.AbilityMoveCount.BaseStat);
		}

		public override void ResetStatsToDefault()
		{
			base.ResetStatsToDefault();
			RuntimeData.AbilityMoveCount.ResetToMax();
		}

		public override void TryCounterAttack(IRuntimeGameObject target, ref DamageType counterDamageType, bool canHandleDie = true)
		{
			// Hero can't counter attack
		}
	}
}