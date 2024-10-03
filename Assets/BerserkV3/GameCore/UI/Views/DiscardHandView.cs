using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public interface IDiscardHandView
	{
		RectTransform MiddleContainer { get; }
		RectTransform BottomContainer { get; }

		void SetInteractable(bool value);
		void SetButtonCallback(Action value);
		void SetButtonVisibility(bool value);
		void SetButtonText(string value);
		void SetHeaderText(string value);

		UniTask ShowAsync();
		UniTask CloseAsync();
	}
	public partial class DiscardHandView : BaseView, IDiscardHandView
	{
		public RectTransform MiddleContainer => MiddleLayout;
		public RectTransform BottomContainer => BottomLayout;

		public void SetInteractable(bool value)
		{
			CanvasGroup.interactable = value;
		}

		public void SetButtonCallback(Action value)
		{
			ConfirmBtn.onClick.RemoveAllListeners();
			ConfirmBtn.onClick.AddListener(() => value?.Invoke());
		}

		public void SetButtonVisibility(bool value)
		{
			SetActive(ConfirmBtn, value);
		}

		public void SetButtonText(string value)
		{
			Set(OKText, value);
		}

		public void SetHeaderText(string value)
		{
			if (!HeaderText)
				return;
			
			SetActive(HeaderText, !string.IsNullOrEmpty(value));
			Set(HeaderText, value);
		}

		public UniTask ShowAsync()
		{
			Show();
			return CanvasGroup.DOFade(1f, 0.5f).From(0f).Play().ToUniTask();
		}

		public async UniTask CloseAsync()
		{
			await CanvasGroup.DOFade(0f, 0.5f).From(1f).Play().ToUniTask();
			Close();
		}

		protected override void OnHidden()
		{
			base.OnHidden();
			ConfirmBtn.onClick.RemoveAllListeners();
		}
		
		protected override void OnClosed()
		{
			base.OnClosed();
			ConfirmBtn.onClick.RemoveAllListeners();
		}
		
		private void OnDestroy()
		{
			ConfirmBtn.onClick.RemoveAllListeners();
		}
	}
}