using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.Buffs;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Auras
{
	[EffectKeyword(EffectKeyword.TalosAura)]
	[EffectKeyword(EffectKeyword.KobaloiRecruiterAura)]
	public class BuffStatReplaceManaAuraEffect : BuffStatReplaceManaEffect
	{		
		protected override void OnDisabled()
		{
			base.OnDisabled();
			if (RuntimeData.Disabled)
			{
				OnExpire();
				return;
			}
			
			Execute();
		}
	}
}