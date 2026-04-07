using Audio;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Vulcan.Audio;
using Mathf = UnityEngine.Mathf;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.TakeHit)]
	public class TakeHitVisual : EffectVisual
	{
		private readonly Vector3 hitIndicationScaleTo = new(1.2f, 1.2f, 1f);
		private readonly Vector3 hitIndicationScaleEnd = new(0f, 0f, 1f);
		private const float HIT_INDICATION_OFFSET_DURATION = 1f;
		private const float HIT_INDICATION_SCALE_DURATION = 0.5f;
		private const float HIT_DURATION = 0.75f;
		private const float HIT_SHAKE_STRENGTH = 12f;

		public override UniTask PlaySingleEffectAsync()
		{
			return UniTask.WhenAll(Targets.Select(t =>
			{
				AudioController.Play(Clip.TableCard_Damage);
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
			var damageValue = Mathf.Abs(arg.From - arg.To);
			var damageText = $"{damageValue}";
			var indicator = (VFXText) VfxApplication.SpawnVfxToParent(EffectVisualKeyword.TakeHit);
			indicator.SetPosition(target.position);
			indicator.SetText(damageText);
			indicator.SetupDetph();

			var vfxTransform = indicator.transform;
			var randomVector = new Vector3(Random.Range(-100, 100), 0, 0);
			var targetOffset = vfxTransform.localPosition + randomVector;
			indicator.SetLocalRotation(Quaternion.identity);
			indicator.SetScale(new Vector3(0f,0f,1f));
			vfxTransform.DOScale(hitIndicationScaleTo, HIT_INDICATION_SCALE_DURATION).SetEase(Ease.OutBounce);
			vfxTransform.DOLocalMove(targetOffset, HIT_INDICATION_OFFSET_DURATION * 2).SetEase(Ease.InQuad);
			vfxTransform.DOScale(hitIndicationScaleEnd, HIT_INDICATION_SCALE_DURATION).SetDelay(HIT_INDICATION_OFFSET_DURATION).SetEase(Ease.InQuad);
			return UniTask.CompletedTask;
		}
	}
}
