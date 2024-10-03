using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.GiveEffects;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Auras
{
	[EffectKeyword(EffectKeyword.ImmortalAura)]
	public class GiveTemporaryEffectsAuraEffect : GiveTemporaryEffectsEffect
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