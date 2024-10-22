#if DOTWEEN
using System;
using System.Threading;
using DG.Tweening;
using UnityEngine;

namespace RR.UIService.AnimationSource
{
	public class UIFadeDoTweenAnimation : UIAnimationBase<UIFadeAnimationConfig>
	{
		private Tween tween;
		private CanvasGroup canvasGroup;

		protected override void OnInit()
		{
			base.OnInit();
			if(!Window.Parent.TryGetComponent(out canvasGroup))
				canvasGroup = Window.Parent.gameObject.AddComponent<CanvasGroup>();
		}

		protected override void OnPlay(Action onCompleted, CancellationToken token)
		{
			if (!canvasGroup)
			{
				onCompleted?.Invoke();
				return;
			}

			var fromTo = Config.FromTo.GetAllowed(Vector2.one * canvasGroup.alpha);
			OnReset(null, token);
			tween = canvasGroup
				.DOFade(fromTo.y, Config.Duration)
				.From(fromTo.x)
				.SetDelay(Config.Delay);

			tween = Config.UseEase
				? tween.SetEase(Config.Ease)
				: tween.SetEase(Config.EaseCurve);

			tween.OnComplete(EndAnimation)
				.OnKill(EndAnimation)
				.Play();

			if (token.CanBeCanceled)
				token.Register(() => OnStop(null, token));

			return;

			void EndAnimation()
			{
				if (onCompleted == null)
					return;

				var mem = onCompleted;
				onCompleted = null;
                
				OnStop(null, token);
				mem?.Invoke();
			}
		}

		protected override void OnStop(Action onCompleted, CancellationToken token)
		{
			if (tween == null || !canvasGroup)
			{
				onCompleted?.Invoke();
				return;
			}

			var mem = tween;
			tween = null;
			mem.Kill();
			onCompleted?.Invoke();
		}

		protected override void OnReset(Action onCompleted, CancellationToken token)
		{
			OnStop(null, token);
			if (!Window?.Content || !canvasGroup)
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
			OnStop(null, CancellationToken.None);
			canvasGroup = null;
		}
	}
}
#endif