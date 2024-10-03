using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Slash)]
	public class SlashEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			var value = ValueModRounded(Executor.RuntimeData.Attack);
			var counterDamageType = DamageType.None;
			foreach (var target in GetExecutionTargets())
			{
				target.TakeDamage(value, Executor, EffectData.DamageType, ref counterDamageType, exclude: EffectPhase.Counterattack);
			}
		}
	}
}