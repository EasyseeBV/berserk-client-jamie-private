using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Silence)]
	public class SilenceEffect : KeywordEffect
	{
		protected override void OnInit()
		{
			base.OnInit();
			Executor.OnBuffEffectAdded += OnAppliedEffectAdded;
		}

		public override void Dispose()
		{
			if (Executor != null)
				Executor.OnBuffEffectAdded -= OnAppliedEffectAdded;
			base.Dispose();
		}

		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				RemoveImposingEffects(target);
				ResetTargetEffects(target, this);
				ResetTargetBuffs(target);
				target.ChangeEffectPhase(EffectPhase.AfterSilence, Executor.RuntimeData.Id, EffectData.DamageType);
			}
		}

		protected override void OnExpire()
		{
			if (Executor != null)
				Executor.OnBuffEffectAdded -= OnAppliedEffectAdded;
			base.OnExpire();
		}

		protected virtual void RemoveImposingEffects(IRuntimeGameObject target, params string[] except)
		{
			if (!target.RuntimeData.ImposingEffects.Any()) 
				return;

			var remove = target.RuntimeData.ImposingEffects.Except(except).ToArray();
			target.RemoveImpossingEffects(remove);
		}

		protected virtual void ResetTargetEffects(IRuntimeGameObject target, params IRuntimeEffect[] except)
		{
			target.ResetEffects(except);
		}

		protected virtual void ResetTargetBuffs(IRuntimeGameObject target)
		{
			target.ResetBuffStats();
		}

		protected virtual void OnAppliedEffectAdded(IRuntimeEffect effect)
		{
			if (effect == this)
				return;
			
			Executor.OnBuffEffectAdded -= OnAppliedEffectAdded;
			Executor.RemoveAppliedEffect(this);
		}
	}

}