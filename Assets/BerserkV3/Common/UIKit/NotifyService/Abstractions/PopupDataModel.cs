using System;

namespace BerserkV3.Common.UIKit.Abstractions
{
	public abstract class PopupDataModel
	{
		public event Action OnRelease;
		public string TitleText { get; protected set; }
		public string BodyText { get; protected set; }
		public string ConfirmButtonText { get; protected set; }
		public string CancelButtonText { get; protected set; }

		public bool IsTitleVisible { get; protected set; }
		public bool IsDescriptionVisible { get; protected set; }
		public bool AreButtonsVisible { get; protected set; }
		public bool AreCancelButtonVisible { get; protected set; }
		public bool AreConfirmButtonVisible { get; protected set; }

		public Action OnCancel { get; protected set; }
		public Action OnClose { get; protected set; }
		public Action OnConfirm { get; protected set; }

		public void Release()
		{
			OnRelease?.Invoke();
		}

		public PopupDataModel SetTitleText(string value)
		{
			TitleText = value;
			return this;
		}

		public PopupDataModel SetBodyText(string value)
		{
			BodyText = value;
			return this;
		}

		public PopupDataModel SetConfirmButtonText(string value)
		{
			ConfirmButtonText = value;
			return this;
		}

		public PopupDataModel SetCancelButtonText(string value)
		{
			CancelButtonText = value;
			return this;
		}

		public PopupDataModel SetTitleVisibility(bool isVisible)
		{
			IsTitleVisible = isVisible;
			return this;
		}

		public PopupDataModel SetDescriptionVisibility(bool isVisible)
		{
			IsDescriptionVisible = isVisible;
			return this;
		}

		public PopupDataModel SetButtonsVisibility(bool areVisible)
		{
			AreButtonsVisible = areVisible;
			return this;
		}

		public PopupDataModel SetCancelButtonVisibility(bool isVisible)
		{
			AreCancelButtonVisible = isVisible;
			return this;
		}

		public PopupDataModel SetConfirmButtonVisibility(bool isVisible)
		{
			AreConfirmButtonVisible = isVisible;
			return this;
		}

		public PopupDataModel SetOnCancel(Action onCancel)
		{
			OnCancel = onCancel;
			return this;
		}

		public PopupDataModel SetOnClose(Action onClose)
		{
			OnClose = onClose;
			return this;
		}

		public PopupDataModel SetOnConfirm(Action onConfirm)
		{
			OnConfirm = onConfirm;
			return this;
		}

		public PopupDataModel SetRelease(Action onRelease)
		{
			OnRelease = onRelease;
			return this;
		}
	}
}