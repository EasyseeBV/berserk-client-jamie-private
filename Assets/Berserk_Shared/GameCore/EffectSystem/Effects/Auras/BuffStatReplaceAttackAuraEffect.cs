using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.Buffs;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Auras
{
	[EffectKeyword(EffectKeyword.MyrmidonTrainerAttackAura)]
	[EffectKeyword(EffectKeyword.ChironAttackAura)]
	public class BuffStatReplaceAttackAuraEffect : BuffStatReplaceAttackEffect
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