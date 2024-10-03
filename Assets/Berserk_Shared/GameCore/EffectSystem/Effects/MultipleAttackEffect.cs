using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Confusing)]
	public class MultipleAttackEffect : KeywordEffect
	{
		public override bool CanExecute()
		{
			return !Executor.IsDead && base.CanExecute();
		}

		protected override void OnExecute()
		{
			StartAttacks();
			var targetsList = GetExecutionTargets().ToList();
			var hitsLeft = RuntimeData.CurrentValue;

			while (hitsLeft > 0 && targetsList.Count > 0)
			{
				if (Executor.IsDead)
					break;

				var target = MoveNextTarget(hitsLeft, targetsList);
				if (target == null || target.IsDead)
				{
					targetsList.Remove(target);
					continue;
				}

				Attack(hitsLeft, target);

				if (target.IsDead)
					targetsList.Remove(target);
				
				hitsLeft--;
			}
			
			EndAttacks();
		}

		protected virtual IRuntimeGameObject MoveNextTarget(int hitsLeft, IList<IRuntimeGameObject> targets)
		{
			if (targets == null || targets.Count == 0)
				return default;
			
			return targets[hitsLeft % targets.Count];
		}

		protected virtual EffectPhase[] GetExcludePhaseOnAttack()
		{
			return EffectData.Phases;
		}

		protected virtual void StartAttacks(){}

		protected virtual void Attack(int hitsLeft, IRuntimeGameObject target)
		{
			var runtimeArgs = GetExcludePhaseOnAttack().Select(p => new EffectPhaseArg(p));
			LogicContext.EffectExecutor.CreateAndExecuteEffect(GetEffectAttackId(), Executor, runtimeArgs, target);
		}
		
		protected virtual void EndAttacks(){}

		protected virtual string GetEffectAttackId()
		{
			return EffectData.GetConfigIdsFromMeta().First();
		}
	}
}