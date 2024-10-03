using System;
using System.Linq;
using Audio;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Core.DebugSystem;
using UnityEngine;
using Vulcan.Audio;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.GenericAttack)]
	public class GenericAttackVisual : EffectVisual
	{
		private const float ANIMATED_ATTACK_SIZE = 1.5f;
		public override async UniTask StartEffectAsync()
		{
			foreach (var target in Targets.Where(t => !t.RuntimeGameObject.IsDead))
				await AnimateAttack(target.SelfContainer);
		}

		private async UniTask AnimateAttack(Transform target)
		{
			if (target == null)
			{
				RRLogger.Error("Target RectTransform == null");
				return;
			}
			if (Executor == null || !Executor.SelfContainer)
			{
				RRLogger.Error("Executor or Executor.SelfContainer is null");
				await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
				return;
			}
			
			var effectStackApplication = Executor switch
			{
				ICardView cardView => cardView.Strategy is CardCreatureStrategy cardCreatureStrategy
					? cardCreatureStrategy.EffectHintsApplication 
					: null,
				
				IHeroView heroView => heroView.EffectHintsApplication,
				_ => throw new NotImplementedException()
			};

			effectStackApplication.Hide();
			var executorTransform = Executor.SelfContainer;
			var fromPosition = executorTransform.position;
			var sibling = executorTransform.GetSiblingIndex();
			var scaleFrom = executorTransform.localScale;
			var scaleTo = scaleFrom * ANIMATED_ATTACK_SIZE;
			executorTransform.SetAsLastSibling();
			executorTransform.DOKill();
			
			try
			{
				await DOTween.Sequence()
					.Append(executorTransform.DOScale(scaleTo, 0.4f).SetEase(Ease.InOutCubic))
					.Append(executorTransform
						.DOMove(target.position, 0.1f)
						.SetDelay(0.05f)
						.SetEase(Ease.InCirc)
						.OnComplete(() => AudioController.Play(Clip.TableCard_Attack)))
					.Append(executorTransform.DOMove(fromPosition, 0.3f).SetEase(Ease.OutCubic))
					.Append(executorTransform.DOScale(scaleFrom, 0.3f).SetEase(Ease.InOutCubic))
					.OnComplete(() => executorTransform.SetSiblingIndex(sibling))
					.SetAutoKill(true)
					.Play();
			}
			catch (Exception e)
			{
				RRLogger.Error("Failed attack animation");
			}
			effectStackApplication.Show();
		}
	}
}