using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.Taunting)]
	public class FaceIdVisual : DefaultAppliedVfxVisual
	{
		public override UniTask ApplyLongEffectAsync()
		{
			return TryGetLayout(Executor, out var layout) 
				? UniTask.WhenAll(layout.AddFaceAsync(GetFaceId()), base.ApplyLongEffectAsync()) 
				: UniTask.CompletedTask;
		}

		public override UniTask ExpireLongEffectAsync()
		{
			return TryGetLayout(Executor, out var layout) 
				? UniTask.WhenAll(layout.RemoveFaceAsync(GetFaceId()), base.ExpireLongEffectAsync()) 
				: UniTask.CompletedTask;
		}

		protected bool TryGetLayout(IRuntimeObjectView target, out ICreatureLayout result)
		{
			result = target switch
			{
				ICardView cardView => result = cardView.Layout as ICreatureLayout,
				IHeroView heroView => heroView.Layout as ICreatureLayout,
				_ => null
			};
			
			return result != null;
		}
		
		protected virtual FaceId GetFaceId()
		{
			return new FaceId(EffectData.VisualKeyword, (int) EffectData.VisualKeyword);
		}
	}
}