using System;
using System.Threading;
using BerserkV3.Generic.ChatWheel;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Generic.Emotions
{
	public partial class EmotionView : BaseView, IChatWheelElement, IEmotionView
	{
		[SerializeField] private float fadeDuratiuon = 0.3f;

		private Tween fadeTween;
		private Transform oldParent;
		private UIFlipBook flipBook;
		private bool showed;
		
		public string Id { get; private set; }

		public bool AutoDispose { get; set; } = true;

		public IChatWheelSlot OccupiedSlot { get; private set; }

		public async UniTask InitAsync(Emotion emotion, CancellationToken token = default)
		{
			if (!FlipBookImage)
				throw new NullReferenceException("FlipBookImage is missing");

			Clear();
			SetActive(this, false);
			Id = emotion.Id;
			flipBook = new UIFlipBook(FlipBookImage);
			try
			{
				await FlipBookImage.LoadResourceAsync(emotion.Uri, token);
				flipBook.Init(emotion.Width, emotion.Height, emotion.TimeScale);
					
				showed = true;
				Show(noAnimation:true);
				FadeOut();
				
				if (emotion.LifeTime <= 0 || !AutoDispose)
					return;
				
				DOVirtual.DelayedCall(emotion.LifeTime, () => FadeIn(Dispose));
			}
			catch (Exception)
			{
				showed = false;
				flipBook.Reset();
				FlipBookImage.ReleaseResource();
			}
		}

		public virtual void Dispose()
		{
			Destroy(gameObject);
		}

		public virtual void Hold(IChatWheelSlot slot)
		{
			OccupiedSlot = slot ?? throw new NullReferenceException("Parent does not exist");
			oldParent = RectTransform.parent;
			
			RectTransform.SetParent(slot.Container);
			RectTransform.anchoredPosition = Vector3.zero;
			RectTransform.localScale = Vector3.one;
		}

		public virtual void Release()
		{
			if (transform)
				transform.SetParent(oldParent);

			OccupiedSlot = null;
		}

		private void FadeIn(Action onComplete = null)
		{
			Fade(0, onComplete);
		}

		private void FadeOut(Action onComplete = null)
		{
			Fade(1, onComplete);
		}

		private void Fade(float alphaTo, Action onComplete = null)
		{
			if(!FlipBookImage)
				return;
			
			fadeTween?.Kill();
			fadeTween = DOTweenModuleUI.DOFade((Graphic) FlipBookImage, alphaTo, fadeDuratiuon)
				.OnComplete(() => onComplete?.Invoke())
				.Play();
		}

		private void Clear()
		{
			OccupiedSlot = null;
			showed = false;
			fadeTween?.Kill();
			fadeTween = null;
			
			flipBook?.Reset();
			flipBook?.Dispose();
			flipBook = null;
			FlipBookImage.ReleaseResource();
		}

		private void Update()
		{
			if(showed)
				flipBook?.Animate();
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}