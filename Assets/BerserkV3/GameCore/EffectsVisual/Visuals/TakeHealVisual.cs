using System;
using Audio;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Vulcan.Audio;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.TakeHeal)]
	public class TakeHealVisual : EffectVisual
	{
		private const float DURATION = .75f;
		public override UniTask PlaySingleEffectAsync()
		{
			return UniTask.WhenAll(Targets.Select(t => 
			{
				AudioController.Play(Clip.TableCard_Heal);
				t.SelfContainer.DOKill();
				var sourceScale = t.SelfContainer.localScale;
				t.SelfContainer
					.DOScale(0.75f, DURATION)
					.SetLoops(2, LoopType.Yoyo)
					.OnKill(() => t.SelfContainer.localScale = sourceScale)
					.Play();
				return UniTask.Delay(TimeSpan.FromSeconds(DURATION/3f));
			}));
		}
	}
}