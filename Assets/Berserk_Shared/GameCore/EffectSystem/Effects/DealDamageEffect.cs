using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Mark)]
	[EffectKeyword(EffectKeyword.Destroy)]
	[EffectKeyword(EffectKeyword.DealDamage)]
	[EffectKeyword(EffectKeyword.DeckDepleted)]
	[EffectKeyword(EffectKeyword.Poisoning)]
	public class DealDamageEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			var counterDamageType = DamageType.None;
			foreach (var target in GetExecutionTargets())
				DealDamage(target, ref counterDamageType, EffectPhase.Counterattack);
		}

		protected virtual void DealDamage(
			IRuntimeGameObject target, 
			ref DamageType counterDamageType, 
			params EffectPhase[] exceptPhases)
		{
			target.TakeDamage(GetDamageValue(target), Executor, EffectData.DamageType, ref counterDamageType, exclude: exceptPhases);
		}

		protected virtual int GetDamageValue(IRuntimeGameObject target)
		{
			var stat = target.RuntimeData.Hp > 0 
				? target.RuntimeData.Hp 
				: target.RuntimeData.Armor;
			
			return ValueModRounded(stat);
		}
	}
}