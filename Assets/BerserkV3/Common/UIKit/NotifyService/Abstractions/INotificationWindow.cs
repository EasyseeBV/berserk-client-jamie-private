using System;

namespace BerserkV3.Common.UIKit.Abstractions
{
	public interface INotificationWindow
	{
		INotificationWindow SetTitleText(string text);
		INotificationWindow SetBodyText(string text);
		INotificationWindow SetConfirmButtonText(string text);
		INotificationWindow SetCancelButtonText(string text);
		INotificationWindow SetTopContentVisibility(bool isVisible);
		INotificationWindow SetMiddleContentVisibility(bool isVisible);
		INotificationWindow SetBottomContentVisibility(bool isVisible);
		INotificationWindow SetCancelButtonVisibility(bool isVisible);
		INotificationWindow SetConfirmButtonVisibility(bool isVisible);
		INotificationWindow SetCancelAction(Action onPress);
		INotificationWindow SetCloseAction(Action onPress);
		INotificationWindow SetConfirmAction(Action onPress);
		INotificationWindow Clear();
	}
}