using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.GiveEffects;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Auras
{
	[EffectKeyword(EffectKeyword.AchillesAura)]
	public class AchillesAuraEffect : GiveTemporaryEffectsOnceEffect
	{
		public override bool CanExecute()
		{
			return IsAllowedToBuff() || IsAllowedToBuffExpire();
		}

		private bool IsAllowedToBuff()
		{
			var allowedByConfigValue = ValueModRounded() >= EffectData.MinTargetCount;
			return allowedByConfigValue && base.CanExecute();
		}

		private bool IsAllowedToBuffExpire()
		{
			var allowedByConfigValue = ValueModRounded() >= EffectData.MinTargetCount;
			return !allowedByConfigValue && !base.CanExecute();
		}

		protected override void OnExecute()
		{
			if (IsAllowedToBuff())
			{
				base.OnExecute();
				return;
			}
			
			if(IsAllowedToBuffExpire())
				OnExpire();
		}

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