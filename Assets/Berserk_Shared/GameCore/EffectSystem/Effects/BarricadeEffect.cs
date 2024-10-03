using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{

	[EffectKeyword(EffectKeyword.Barricade)]
	public class BarricadeEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				RemoveImposingEffects(target);
				ResetTargetEffects(target, this);
			}
		}

		protected override void OnExpire()
		{
			base.OnExpire();
			foreach (var target in GetExecutionTargets())
			{
				ResetTargetEffects(target); // Important! remove effect before die
				target.TryDie(Executor, EffectData.DamageType);
			}
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
	}
}