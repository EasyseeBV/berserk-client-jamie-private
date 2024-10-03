using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using BerserkV3.GameCore.EffectsVisual.Models;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	#region Keyword
	[EffectVisual(EffectVisualKeyword.Sleeping)]

	#endregion Keyword

	#region Cards
	
	[EffectVisual(EffectVisualKeyword.Poisoning)]
	[EffectVisual(EffectVisualKeyword.Stealth)]
	[EffectVisual(EffectVisualKeyword.Sanctification)]

	#endregion

	public class DefaultAppliedVfxVisual : EffectVisual
	{
		private VFXView vfxView;

		private CommonAppliedConfig VisualConfig => VisualConfigRepository.CommonApplied;

		public override UniTask ApplyLongEffectAsync()
		{
			vfxView = VfxApplication.SpawnVfx(EffectData.VisualKeyword, Executor.SelfContainer);
			return UniTask.Delay(VisualConfig.ApplicationDelayMs);
		}

		public override UniTask ExpireLongEffectAsync()
		{
			SoftStop();
			return UniTask.Delay(VisualConfig.ExpirationDelayMs);
		}

		public override void SoftStop()
		{
			vfxView.Value()?.SoftStop();
		}

		public override void Dispose()
		{
			vfxView.Value()?.DestroyInstance();
		}
	}
}