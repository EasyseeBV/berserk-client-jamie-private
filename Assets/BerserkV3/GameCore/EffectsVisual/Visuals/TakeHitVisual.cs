using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using BerserkV3.Common.AudioSystem;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.TakeHit)]
	public class TakeHitVisual : EffectVisual
	{
		private readonly IAudioApplication audioApplication;
		private readonly Vector3 hitIndicationScaleTo = new(1.2f, 1.2f);
		private readonly Vector3 hitIndicationScaleEnd = Vector3.zero;
		private const float HIT_INDICATION_OFFSET_DURATION = 1f;
		private const float HIT_INDICATION_SCALE_DURATION = 0.5f;
		private const float HIT_DURATION = 0.75f;
		private const float HIT_SHAKE_STRENGTH = 12f;
		
		public TakeHitVisual(IAudioApplication audioApplication)
		{
			this.audioApplication = audioApplication;
		}

		public override UniTask PlaySingleEffectAsync()
		{
			return UniTask.WhenAll(Targets.Select(t =>
			{
				audioApplication.PlaySound(Clip.TableCard_Damage);
				t.SelfContainer.DOKill();
				var sourcePosition = t.SelfContainer.localPosition;
				t.SelfContainer
					.DOShakePosition(HIT_DURATION, HIT_SHAKE_STRENGTH)
					.OnKill(() => t.SelfContainer.localPosition = sourcePosition)
					.Play();

				return HitIndicationAsync();
			}));
		}
		
		private UniTask HitIndicationAsync()
		{
			var target = Executor.SelfContainer;
			var arg = Model.GetRuntimeArg<ObjectStatEffectArg>();
			var damageText = $"{-arg.Value}";
			var indicator = (VFXText) VfxApplication.SpawnVfxToParent(EffectVisualKeyword.TakeHit);
			indicator.SetPosition(target.position);
			indicator.SetText(damageText);
			indicator.SetupDetph();

			var vfxTransform = indicator.transform;
			var randomVector = new Vector3(Random.Range(-100, 100), 200, 0);
			var targetOffset = vfxTransform.localPosition + randomVector;
			indicator.SetLocalRotation(Quaternion.identity);
			indicator.SetScale(Vector3.zero);
			vfxTransform.DOScale(hitIndicationScaleTo, HIT_INDICATION_SCALE_DURATION).SetEase(Ease.OutBounce);
			vfxTransform.DOLocalMove(targetOffset, HIT_INDICATION_OFFSET_DURATION * 2).SetEase(Ease.InQuad);
			vfxTransform.DOScale(hitIndicationScaleEnd, HIT_INDICATION_SCALE_DURATION).SetDelay(HIT_INDICATION_OFFSET_DURATION).SetEase(Ease.InQuad);
			return UniTask.CompletedTask;
		}
	}
}