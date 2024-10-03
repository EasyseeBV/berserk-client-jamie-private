using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.ImmuneToDamage)]
	public class ImmuneToDamageEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				foreach (var damageValue in GetDamageValues(target))
				{
					if (target.RuntimeData.ImmuneToDamage.Contains(damageValue))
						continue;

					target.RuntimeData.ImmuneToDamage.Add(damageValue);
					LogicContext.LogicQueueController.Add(new AddImmuneToDamage(damageValue, target.RuntimeData.Id),
						target.GetAccessibleReceiver());
				}
			}
		}

		protected override void OnExpire()
		{
			foreach (var target in GetExecutionTargets())
			{
				foreach (var damageValue in GetDamageValues(target))
				{
					if (target.RuntimeData.ImmuneToDamage.Remove(damageValue))
						LogicContext.LogicQueueController.Add(new DeleteImmuneToDamage(damageValue, target.RuntimeData.Id), target.GetAccessibleReceiver());
				}
			}

			base.OnExpire();
		}

		protected virtual IEnumerable<DamageValue> GetDamageValues(IRuntimeGameObject target)
		{
			return EffectData.DamageType == DamageType.None 
				? Array.Empty<DamageValue>() 
				: new[] {new DamageValue(EffectData.DamageType, EffectData.ValueMod, RuntimeData.Id, RuntimeData.CurrentValue)};
		}
	}
}