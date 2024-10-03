using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.GenericAttack)]
	public class GenericAttackEffect : KeywordEffect
	{
		public override bool CanExecute()
		{
			if (!base.CanExecute())
				return false;

			SetTargets(Targets?.Where(x => !x.IsDead && !x.HasEffectsDisable()).ToArray() ?? Array.Empty<IRuntimeGameObject>());
			return base.CanExecute();
		}

		protected override void OnExecute()
		{
			var phases = GetExcludePhaseFromMeta();
			var counterDamageType = DamageType.None;
			foreach (var target in GetExecutionTargets())
			{
				if (target.IsDead || target.HasEffectsDisable())
					continue;

				if (Executor.IsDead || Executor.HasEffectsDisable())
					break;

				Executor.Attack(target, EffectData.DamageType, ref counterDamageType, exclude: phases);

				if (Executor.IsDead || Executor.HasEffectsDisable())
					break;
			}
		}

		protected virtual EffectPhase[] GetExcludePhaseFromMeta()
		{
			return RuntimeData.GetRuntimeArgs<EffectPhaseArg>().Select(x => x.EffectPhase).ToArray();
		}
	}
}