using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.Buffs;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Auras
{
	[EffectKeyword(EffectKeyword.MyrmidonTrainerHpAura)]
	[EffectKeyword(EffectKeyword.KallistoAura)]
	public class BuffStatReplaceHpAuraEffect : BuffStatReplaceHealthEffect
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