using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Reflect)]
	public class ReflectEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			var counterDamageType = DamageType.None;
			foreach (var target in GetExecutionTargets())
			{
				if (target.Data.Type is ObjectType.Spell)
					continue;
				
				target.TakeDamage(target.RuntimeData.Attack, Executor, EffectData.DamageType, ref counterDamageType, exclude: EffectPhase.Counterattack);
			}
		}
	}
}