using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.Destruction)]
	public class DestructionVisual : DefaultSingleVfxVisual
	{
		public override async UniTask StartEffectAsync()
		{
			await base.StartEffectAsync();
			foreach (var target in Targets)
			{
				if (target.RuntimeData.Type == ObjectType.Hero)
					target.Disable();
			}
		}
	}
}