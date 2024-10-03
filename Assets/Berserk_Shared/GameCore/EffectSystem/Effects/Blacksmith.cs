using System.Linq;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Blacksmith)]
	public class Blacksmith : KeywordEffect
	{
		protected override void OnExecute()
		{
			var maxAttackOnTable = Targets
				.Select(x => x.RuntimeData.Attack.TotalMax)
				.DefaultIfEmpty(0)
				.Max();
			
			foreach (var target in GetExecutionTargets())
			{
				var statAttack = target.RuntimeData.Attack;
				if (maxAttackOnTable <= 0)
					continue;
				
				var modifier = new SimpleModifierInt()
					.SetMaxModifier(maxAttackOnTable)
					.SetCurrModifier(maxAttackOnTable)
					.SetModifierId(RuntimeData.Id.ToString());
				statAttack.AddModifier(modifier);
			}
		}
	}
}