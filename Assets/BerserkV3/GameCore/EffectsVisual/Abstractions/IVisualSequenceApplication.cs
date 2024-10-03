using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.EffectsVisual.Abstractions
{
	public interface IVisualSequenceApplication
	{
		UniTask PlaySingleSequenceAsync(IRuntimeEffectModel model);

		UniTask StartEffectAsync(IRuntimeEffectModel model);

		UniTask EndEffectAsync(IRuntimeEffectModel model);

		UniTask ApplyLongEffectAsync(IRuntimeEffectModel model, bool resore);

		void InitLongEffect(IRuntimeEffectModel model);
		
		UniTask ChangeEffectAsync(IRuntimeEffectModel model);
		
		UniTask ExpireLongEffectAsync(IRuntimeEffectModel model);
	}
}