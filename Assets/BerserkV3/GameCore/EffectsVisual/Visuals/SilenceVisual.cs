using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.Silence)]
	public class SilenceVisual : DefaultSingleVfxVisual
	{
		private readonly IVisualSequenceApplication visualSequenceApplication;
		private readonly IRuntimeEffectModelFatory effectModelFatory;

		public SilenceVisual(
			IVisualSequenceApplication visualSequenceApplication,
			IRuntimeEffectModelFatory effectModelFatory)
		{
			this.visualSequenceApplication = visualSequenceApplication;
			this.effectModelFatory = effectModelFatory;
		}

		public override UniTask StartEffectAsync()
		{
			return UniTask.WhenAll(Targets
				.SelectMany(x=> x.RuntimeData.AppliedEffects)
				.Select(effectModelFatory.Create)
				.ToArray()
				.Select(visualSequenceApplication.ExpireLongEffectAsync)
				.Append(base.StartEffectAsync()));
		}
	}
}