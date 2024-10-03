using System.Collections;
using UI;

namespace Game.Entities
{
	public interface IInteractiveEntity : IMonoEntity
	{
		EntityView EntityView { get; }

		void KillSelf();

		void ApplyDamage(IInteractiveEntity source, int value);

		void Attack(IInteractiveEntity target, bool defenceDamage = true);

		IEnumerator YieldAttack(IInteractiveEntity target, bool defenceDamage = true);

		bool IsCanAttack { get; set; }
		bool IsAllowedAttack { get; set; }
	}
}