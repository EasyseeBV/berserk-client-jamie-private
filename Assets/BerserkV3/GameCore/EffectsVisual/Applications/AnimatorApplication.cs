using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Core.DebugSystem;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.EffectsVisual.Applications
{
	public class AnimatorApplication : IAnimatorApplication
	{
		private readonly IActionsQueue actionsQueue;
		private readonly IHoveringSystem hoveringSystem;
		
		public AnimatorApplication(IActionsQueue actionsQueue, IHoveringSystem hoveringSystem)
		{
			this.actionsQueue = actionsQueue;
			this.hoveringSystem = hoveringSystem;
		}

		private static UniTask RestorePositionAsync(TargetTransform target,
		                                            Transform cardTransform,
		                                            CancellationToken token,
		                                            float duration,
		                                            Ease ease)
		{
			try
			{
				cardTransform.DOKill();
				return DOTween.Sequence()
					.Append(cardTransform.DOLocalMove(target.Position, duration))
					.Join(cardTransform.DOLocalRotateQuaternion(target.Rotation, duration))
					.SetEase(ease)
					.Play()
					.ToUniTask(cancellationToken: token);
			}
			catch (OperationCanceledException)
			{
				//ignore
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
			return UniTask.CompletedTask;
		}

		public async UniTask RestorePositionsAsync(IDictionary<ICardView, TargetTransform> targets,
		                                           CancellationToken token,
		                                           Ease ease = Ease.InExpo,
		                                           float duration = 0.2f)
		{
			
			if(targets == null || targets.Count == 0)
				return;

			hoveringSystem.Cancel(); // cancel hovering to reset sibling indices
			hoveringSystem.Enable(false);
			try
			{
				using var db = DisposeBlock.Spawn();
				var animations = db.SpawnList<UniTask>();

				foreach (var (cardView, targetTransform) in targets)
				{
					if (cardView is CardViewMock or null)
						continue;

					var cardTransform = cardView.SelfContainer;

					animations.Add(RestorePositionAsync(targetTransform, cardTransform, token, duration, ease));
				}

				await UniTask.WhenAll(animations).AttachExternalCancellation(token);
			}
			catch (OperationCanceledException)
			{
				// ignore
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
			hoveringSystem.Enable(true);
		}

		public void EnqueueRestorePosition(TargetTransform target,
		                                   Transform source,
		                                   TweenCallback onDone = null,
		                                   Ease ease = Ease.InExpo,
		                                   float duration = 0.5f)
		{
			actionsQueue.Enqueue(async () =>
			{
				hoveringSystem.Cancel(); // cancel hovering to reset sibling indices
				hoveringSystem.Enable(false);
				await RestorePositionAsync(target, source, CancellationToken.None, duration, ease);
				hoveringSystem.Enable(true);
				
				onDone?.Invoke();
			});
		}

		public async UniTask RearrangeAsync(
			IDictionary<ICardView, TargetTransform> targets, 
			CancellationToken token, 
			Ease ease = Ease.OutExpo, 
			float duration = 0.55f)
		{
			if(targets == null || targets.Count == 0 || token.IsCancellationRequested)
				return;
			
			hoveringSystem.Cancel(); // cancel hovering to reset sibling indices
			hoveringSystem.Enable(false);
			try
			{
				using var db = DisposeBlock.Spawn();
				var animations = db.SpawnList<UniTask>();

				foreach (var (cardView, targetTransform) in targets.OrderBy(x => x.Key.RuntimeData.RelativePositionX))
				{
					if (token.IsCancellationRequested)
					{
						db.Dispose();
						animations.Clear();
						return;
					}

					if (cardView is CardViewMock or null)
						continue;

					var cardTransform = cardView.SelfContainer;
					cardTransform.DOKill();
					cardView.TargetTransform = targetTransform;
					cardTransform.SetSiblingIndex(cardView.RuntimeData.RelativePositionX);
					var animationTask = DOTween.Sequence()
						.Join(cardTransform.DOLocalMove(targetTransform.Position, duration))
						.Join(cardTransform.DOLocalRotateQuaternion(targetTransform.Rotation, duration))
						.SetEase(ease)
						.Play()
						.ToUniTask(cancellationToken: token);

					animations.Add(animationTask);
				}

				await UniTask.WhenAll(animations);
			}
			catch (OperationCanceledException)
			{
				// ignore
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
			hoveringSystem.Enable(true);
		}
	}
}
