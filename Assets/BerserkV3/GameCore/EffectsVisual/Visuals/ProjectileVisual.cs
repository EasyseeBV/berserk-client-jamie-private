using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.Revenge)]
	public class ProjectileVisual : EffectVisual
	{
		public override UniTask StartEffectAsync()
		{
			using var db = DisposeBlock.Spawn();
			var animations = db.SpawnList<UniTask>();

			animations.AddRange(Targets.Select(target =>
				VfxApplication.PlaySingleVFXAsync(EffectData.VisualKeyword, Executor.SelfContainer, target.SelfContainer)));

			return UniTask.WhenAll(animations);
		}
	}
}