using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.RuntimeObjects
{
	public class RuntimeSpellCard : RuntimeGameCard
	{
		public override bool IsDead => RuntimeData.State is RuntimeState.InDiscard or RuntimeState.InExile;

		public override void Attack(
			IRuntimeGameObject target,
			DamageType damageType,
			ref DamageType counterDamageType,
			bool canHandleDie = true,
			params EffectPhase[] exclude)
		{
			// Spells can't attack
		}

		public override void TakeDamage(
			int damage, 
			IRuntimeGameObject initiator, 
			DamageType damageType, 
			ref DamageType counterDamageType, 
			bool canHandleDie = true,
			params EffectPhase[] exclude)
		{
			// Spells can't TakeDamage
		}

		public override void TryCounterAttack(
			IRuntimeGameObject target,
			ref DamageType counterDamageType, 
			bool canHandleDie = true)
		{
			// Spells can't counter attack
		}

		public override void Spawn(bool notify = true)
		{
			base.Spawn(false);
			ReturnToDiscard(); // the spell cannot appear on the table, it will be discarded.
		}
	}
}