using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.Common.UIKit
{
	public partial class HexagoneItemView : BaseView
	{
		[Header("Custom Fields")]
		[SerializeField] protected float scaleWhenSelected =  1.05f;
		[SerializeField] protected float scaleDuration = 0.3f;
		[SerializeField] protected Ease scaleEase = Ease.InOutCubic;
		[SerializeField] protected Color whenLockActive = new(0.2f, 0.2f, 0.2f, 0.8f);
		
		private Tween selectTween;
		private Color mainInitColor;
		private Color borderInitColor;
		private Vector3 initialScale;
		private bool initialized;
		public event Action<HexagoneItemView> OnClicked;
		public string Id { get; private set; }
		
		private void OnDestroy()
		{
			selectTween?.Kill();
			selectTween = null;
			OnClicked = null;
			Id = string.Empty;
			MainImage.ReleaseResource();
			HexBorderImage.ReleaseResource();
			HexMaskImage.ReleaseResource();
		}

		protected override void OnClosed()
		{
			selectTween?.Kill();
			selectTween = null;
			OnClicked = null;
			Id = string.Empty;
			MainImage.color = mainInitColor;
			HexBorderImage.color = borderInitColor;
			transform.localScale = initialScale;
			HexButton.onClick.RemoveAllListeners();
		}

		public UniTask InitAsync(
			string id, 
			string mainArtUrl, 
			string frameArtUrl, 
			string maskArtUrl, 
			CancellationToken token = default)
		{
			if (!initialized)
			{
				initialized = true;
				mainInitColor = MainImage.color;
				borderInitColor = HexBorderImage.color;
				initialScale = transform.localScale;
			}

			Id = id;
			OnClicked = null;
			HexButton.onClick.RemoveAllListeners();
			Subscribe(HexButton, () => OnClicked?.Invoke(this));
			return UniTask.WhenAll(
				HexMaskImage.LoadResourceAsync(maskArtUrl, token),
				HexBorderImage.LoadResourceAsync(frameArtUrl, token),
				MainImage.LoadResourceAsync(mainArtUrl, token));
		}

		public void SetInteractable(bool value)
		{
			if (!HexButton)
				return;
			
			SetInteractable(HexButton, value);
		}

		public void SetActiveLock(bool value)
		{
			SetActive(HexLockImage, value);
			Set(MainImage, value ? whenLockActive : mainInitColor);
		}
		
		public void SetActiveBottomText(bool value)
		{
			SetActive(HexBottomImg, value);
		}
		
		public void SetBottomText(string value)
		{
			Set(BottomText, value);
		}

		public void SetBorderColor(Color value)
		{
			Set(HexBorderImage, value);
		}

		public void Select(bool value)
		{
			if (value)
			{
				Select();
				return;
			}
			Deselect();
		}

		public void Select()
		{
			selectTween?.Kill();
			var localScale = transform.localScale;
			selectTween = transform
				.DOScale(localScale * scaleWhenSelected, scaleDuration)
				.From(initialScale)
				.SetEase(scaleEase)
				.SetAutoKill(true)
				.Play();
		}

		public void Deselect()
		{
			selectTween?.Kill();
			selectTween = transform
				.DOScale(initialScale, scaleDuration)
				.From(transform.localScale)
				.SetEase(scaleEase)
				.SetAutoKill(true)
				.Play();
		}
	}
}