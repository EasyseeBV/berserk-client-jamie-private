using System;
using BerserkV3.Common.UIKit.Abstractions;
using RR.UIService;
using RR.UIService.FullFade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit.NotifyService.Realizations
{
	public class NotificationWindowBase : UIWindowBase, INotificationWindow, IFullFadeTarget
	{
		[SerializeField] protected Button СonfirmButton;
		[SerializeField] protected Button СancelButton;
		[SerializeField] protected Button СloseButton;

		[SerializeField] protected TMP_Text TitleTextLabel;
		[SerializeField] protected TMP_Text BodyTextLabel;
		[SerializeField] protected TMP_Text ConfirmButtonTextLabel;
		[SerializeField] protected TMP_Text CancelButtonLabel;

		[SerializeField] protected GameObject TopContentLayout;
		[SerializeField] protected GameObject MiddleContentLayout;
		[SerializeField] protected GameObject BottomContentLayout;

		protected override void OnDisposed()
		{
			Clear();
			base.OnDisposed();
		}

		public override void Hidden()
		{
			Clear();
			base.Hidden();
		}

		public INotificationWindow SetTitleText(string text)
		{
			if (TitleTextLabel)
				TitleTextLabel.text = text;
			return this;
		}

		public INotificationWindow SetBodyText(string text)
		{
			if (BodyTextLabel)
				BodyTextLabel.text = text;
			return this;
		}

		public INotificationWindow SetConfirmButtonText(string text)
		{
			if (ConfirmButtonTextLabel)
				ConfirmButtonTextLabel.text = text;
			return this;
		}

		public INotificationWindow SetCancelButtonText(string text)
		{
			if (CancelButtonLabel)
				CancelButtonLabel.text = text;
			return this;
		}

		public INotificationWindow SetTopContentVisibility(bool isVisible)
		{
			if (TopContentLayout)
				TopContentLayout.SetActive(isVisible);
			return this;
		}

		public INotificationWindow SetMiddleContentVisibility(bool isVisible)
		{
			if (MiddleContentLayout)
				MiddleContentLayout.SetActive(isVisible);
			return this;
		}

		public INotificationWindow SetBottomContentVisibility(bool isVisible)
		{
			if (BottomContentLayout)
				BottomContentLayout.SetActive(isVisible);
			return this;
		}

		public INotificationWindow SetCancelButtonVisibility(bool isVisible)
		{
			if (СancelButton)
				СancelButton.gameObject.SetActive(isVisible);
			return this;
		}

		public INotificationWindow SetConfirmButtonVisibility(bool isVisible)
		{
			if (СonfirmButton)
				СonfirmButton.gameObject.SetActive(isVisible);
			return this;
		}

		public INotificationWindow SetCancelAction(Action onPress)
		{
			if (СancelButton)
				СancelButton.onClick.AddListener(() => onPress?.Invoke());
			return this;
		}

		public INotificationWindow SetCloseAction(Action onPress)
		{
			if (СloseButton)
				СloseButton.onClick.AddListener(() => onPress?.Invoke());
			return this;
		}

		public INotificationWindow SetConfirmAction(Action onPress)
		{
			if (СonfirmButton)
				СonfirmButton.onClick.AddListener(() => onPress?.Invoke());
			return this;
		}

		public INotificationWindow Clear()
		{
			if (СancelButton)
				СancelButton.onClick.RemoveAllListeners();

			if (СonfirmButton)
				СonfirmButton.onClick.RemoveAllListeners();

			if (СloseButton)
				СloseButton.onClick.RemoveAllListeners();

			return this;
		}

		#region IFullFadeTarget implementation

		public Color? FadeColor => null;

		public void OnFadeClick()
		{
		}

		#endregion
	}
}