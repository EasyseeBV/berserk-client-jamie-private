using System;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.AudioSystem;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.TakeHeal)]
	public class TakeHealVisual : EffectVisual
	{
		private readonly IAudioApplication audioApplication;
		private const float DURATION = .75f;
		public TakeHealVisual(IAudioApplication audioApplication)
		{
			this.audioApplication = audioApplication;
		}

		public override UniTask PlaySingleEffectAsync()
		{
			return UniTask.WhenAll(Targets.Select(t => 
			{
				audioApplication.PlaySound(Clip.TableCard_Heal);
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