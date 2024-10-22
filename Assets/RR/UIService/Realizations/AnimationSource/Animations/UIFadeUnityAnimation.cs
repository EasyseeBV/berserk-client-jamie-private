#if !DOTWEEN
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace RR.UIService.AnimationSource
{
	public class UIFadeUnityAnimation : UIAnimationBase<UIFadeAnimationConfig>
	{
		private Coroutine tween;
		private CanvasGroup canvasGroup;

		protected override void OnInit()
		{
			base.OnInit();
			if(!Window.Parent.TryGetComponent(out canvasGroup))
				canvasGroup = Window.Parent.gameObject.AddComponent<CanvasGroup>();
		}
		protected override void OnPlay(Action onCompleted, CancellationToken token)
		{
			OnReset(() => tween = UIAnimationCoroutineRunner.StartCoroutine(OnPlayAsync(onCompleted, token)), token);
		}

		private IEnumerator OnPlayAsync(Action onCompleted, CancellationToken token)
		{
			if (token.IsCancellationRequested || !canvasGroup)
				EndAnimation();
			
			if (Config.Delay > 0)
				yield return new WaitForSeconds(Config.Delay);
            
			if (token.IsCancellationRequested || !canvasGroup)
				EndAnimation();
			
			var elapsedTime = 0f;
			var duration = Config.Duration;
			var ease = Config.EaseCurve;
			var fromTo = Config.FromTo.GetAllowed(Vector2.one * canvasGroup.alpha);
			while (Application.isPlaying && elapsedTime < duration && canvasGroup && !token.IsCancellationRequested)
			{
				elapsedTime += Time.deltaTime;
				var t = ease.Evaluate(elapsedTime / duration);

				if (!canvasGroup || token.IsCancellationRequested)
					EndAnimation();

				canvasGroup.alpha = Mathf.LerpUnclamped(fromTo.x, fromTo.y, t);
				yield return null;
			}

			EndAnimation(fromTo.y);
            
			yield break;
			void EndAnimation(float? end = default)
			{
				if (end.HasValue && canvasGroup)
					canvasGroup.alpha = end.Value;
                
				OnStop(null, token);
				onCompleted?.Invoke();
			}
		}
        
		protected override void OnStop(Action onCompleted, CancellationToken token)
		{
			UIAnimationCoroutineRunner.StopCoroutine(tween);
			tween = null;
			onCompleted?.Invoke();
		}
        
		protected override void OnReset(Action onCompleted, CancellationToken token)
		{
			OnStop(null, token);
			if (!canvasGroup)
			{
				onCompleted?.Invoke();
				return;
			}

			var fromTo = Config.FromTo.GetAllowed(Vector2.one * canvasGroup.alpha);
			canvasGroup.alpha = fromTo.y;
			onCompleted?.Invoke();
		}

		protected override void OnDisposed()
		{
			canvasGroup = null;
			OnStop(null, CancellationToken.None);
		}
	}
}
#endif