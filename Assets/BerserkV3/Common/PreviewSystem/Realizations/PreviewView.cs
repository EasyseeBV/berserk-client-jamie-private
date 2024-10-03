using System;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Core.DebugSystem;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.Common.PreviewSystem
{
	public partial class PreviewView : BaseView, IPreviewView
	{
		private IPreviewData current;
		private CancellationTokenSource previewLoading;
		private Tween closeTween;

		protected override void OnAwake()
		{
			base.OnAwake();
			this.SetHintTarget($"{TutorialTrigger.PreviewCard}").SetTransitionFactorSize().Init().SizeScale *= 1.1f;
			Close();
		}

		public void InitAndShowSide(IPreviewData data, float lifeTime)
		{
			Init(data, SideContainer, lifeTime);
			Show(noAnimation: true);
		}

		public void InitAndShowCenter(IPreviewData data, float lifeTime)
		{
			Init(data, CenterContainer, lifeTime);
			Show(noAnimation: true);
		}

		public void InitAndShowCorner(IPreviewData data, float lifeTime)
		{
			Init(data, CornerContainer, lifeTime);
			Show(noAnimation: true);
		}
		
		public void ShowAndFitAroundTarget(GameObject target, IPreviewData data, float lifeTime)
		{
			Init(data, CornerContainer, lifeTime);
			PositioningAroundTarget(CardHandArtView.RectTransform, target);
			Show(noAnimation: true);
		}

		public void ShowDeckValueInfo(GameObject target, IPreviewData data, float lifeTime)
		{
			InitInfo(data, target, lifeTime);
			Show(noAnimation: true);
		}

		private void Init(IPreviewData data, RectTransform parent, float lifeTime = -1)
		{
			if (data is not IPreviewCardData runtimePreview)
				throw new NotImplementedException($"Can't handle unknown data : {data}");

			RefreshPreviewData(data);
			ConfigureViewPlacement(CardHandArtView, parent);

			CardHandArtView.SetupAsync(runtimePreview.ToCardDataAdapter(), previewLoading.Token)
				.ContinueWith(() =>
				{
					data.OnUpdatePreview += OnRefreshDataView;
					CardHandArtView.SetActive(true);
					if (lifeTime > 0)
						closeTween = DOVirtual.DelayedCall(lifeTime, () => Close());
				})
				.Forget();
		}

		private void InitInfo(IPreviewData data, GameObject parent, float lifeTime = -1)
		{
			RefreshPreviewData(data);
			DeckValuePreviewView.RectTransform.position = parent.transform.position;
			DeckValuePreviewView.Show(data);
			if (lifeTime > 0)
				closeTween = DOVirtual.DelayedCall(lifeTime, () => Close());
		}

		private void ConfigureViewPlacement(BaseView view, RectTransform parent)
		{
			view.RectTransform.localPosition = parent.localPosition;
			view.RectTransform.localScale = parent.localScale;
		}

		private void RefreshPreviewData(IPreviewData data)
		{
			closeTween?.Kill();
			CanvasGroup.alpha = 1;
			previewLoading?.Cancel();
			previewLoading?.Dispose();
			previewLoading = new CancellationTokenSource();
			current = data;
		}

		private void PositioningAroundTarget(RectTransform viewTransform, GameObject target)
		{
			if (!viewTransform)
				return;

			if (!target)
			{
				RRLogger.Error("Can't reposition around target, because target is missing!");
				return;
			}

			var spaceBetweenTarget = 15f;
			var boundsOffset = new Vector2(-25f, -25f);
			var previewTransform = viewTransform;
			var targetTransform = target.transform;

			var previewSize = previewTransform.rect.size * previewTransform.localScale;
			var screenBounds = ((RectTransform.rect.size / 2f) - (previewSize / 2f)) + boundsOffset;
			Vector2 targetSize = targetTransform is RectTransform targetRectTransform
				? targetRectTransform.rect.size * targetRectTransform.localScale
				: targetTransform.localScale;

			var previewOffset = Mathf.Abs((targetSize.x / 2f + previewSize.x / 2f) + spaceBetweenTarget);
			var finalPosition = RectTransform.InverseTransformPoint(targetTransform.position);
			// when X less than zero - then preview on the right side
			// when X greater than zero - then preview on the left side
			finalPosition.x += finalPosition.x < 0 ? previewOffset : -previewOffset;
			finalPosition.x = Mathf.Clamp(finalPosition.x, -screenBounds.x, screenBounds.x);
			finalPosition.y = Mathf.Clamp(finalPosition.y, -screenBounds.y, screenBounds.y);
			previewTransform.localPosition = finalPosition;
		}

		public void Close(bool noAnimation = true)
		{
			current?.Dispose();
			current = null;
			closeTween?.Kill();
			CanvasGroup.alpha = 0;
			base.Close(noAnimation: noAnimation, onAnimationDone: () =>
			{
				previewLoading?.Cancel();
				previewLoading?.Dispose();
				previewLoading = null;
				CardHandArtView.SetActive(false);
				if (DeckValuePreviewView) 
					DeckValuePreviewView.Close();
				CardHandArtView.Release();
			});
		}

		private void OnRefreshDataView()
		{
			if (current == null)
				return;

			if (current is not ICardData cardData)
				throw new NotImplementedException($"Can't handle unknown data : {current}");

			CardHandArtView.Refresh(cardData.ToCardDataAdapter());
		}

		private void OnDestroy()
		{
			current?.Dispose();
			current = null;
			closeTween?.Kill();
			closeTween = null;
			CardHandArtView.Release();
		}
	}
}