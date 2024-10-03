using System.Collections.Generic;
using System.Threading;
using BerserkV3.GameCore.Cards;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Abstractions
{
	public interface IAnimatorApplication
	{
		UniTask RearrangeAsync(
			IDictionary<ICardView, TargetTransform> targets, 
			CancellationToken token, 
			Ease ease = Ease.OutExpo, 
			float duration = 0.55f);

		UniTask RestorePositionsAsync(IDictionary<ICardView, TargetTransform> targets, 
			CancellationToken token, 
			Ease ease = Ease.InExpo,
			float duration = 0.5f);
		
		void EnqueueRestorePosition(TargetTransform target, 
			Transform source, 
			TweenCallback onDone = null, 
			Ease ease = Ease.InExpo,
			float duration = 0.2F);
	}
}