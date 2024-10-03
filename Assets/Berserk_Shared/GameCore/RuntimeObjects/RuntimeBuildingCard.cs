using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.RuntimeObjects
{
	public class RuntimeBuildingCard : RuntimeGameCard
	{
		public override bool IsDead => RuntimeData.State is RuntimeState.InDiscard or RuntimeState.InExile 
		                               || RuntimeData.Armor <= 0;

		public override void Attack(
			IRuntimeGameObject target, 
			DamageType damageType, 
			ref DamageType counterDamageType,
			bool canHandleDie = true, 
			params EffectPhase[] exclude)
		{
			// Buildings can't attack
		}

		public override void TryCounterAttack(
			IRuntimeGameObject target, 
			ref DamageType counterDamageType,
			bool canHandleDie = true)
		{
			// Buildings can't counter attack
		}

		protected override IntStat GetRestoreStat(int value, IRuntimeGameObject initiator)
		{
			return RuntimeData.Armor;
		}
	}

}