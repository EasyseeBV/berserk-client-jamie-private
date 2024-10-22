using System;
using BerserkV3.Common.UIKit.Abstractions;

namespace BerserkV3.Common.UIKit.NotifyService.Models
{
	public class InfoPopupDataModel : PopupDataModel
	{
		public InfoPopupDataModel()
		{
			SetTitleText("ERROR")
				.SetBodyText("SOMETHING WENT WRONG")
				.SetConfirmButtonText("OK")
				.SetCancelButtonText("CANCEL")
				.SetTitleVisibility(true)
				.SetDescriptionVisibility(true)
				.SetButtonsVisibility(true)
				.SetCancelButtonVisibility(false)
				.SetConfirmButtonVisibility(true);
		}
	}
}