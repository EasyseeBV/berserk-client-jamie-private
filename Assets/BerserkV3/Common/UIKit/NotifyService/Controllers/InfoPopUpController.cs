using System.Threading.Tasks;
using BerserkV3.Common.UIKit.Abstractions;
using BerserkV3.Common.UIKit.NotifyService.Models;
using BerserkV3.Common.UIKit.NotifyService.Realizations;
using Cysharp.Threading.Tasks;
using RR.UIService;

namespace BerserkV3.Common.UIKit.NotifyService.Controllers
{
	public class InfoPopUpController : IPopUpController 
	{
		private readonly IUIService uiService;
		private readonly INotifyService notifyService;
		
		private InfoPopupDataModel data;
		
		public PopupDataModel Data => data;
		
		public InfoPopUpController(IUIService uiService, 
			INotifyService notifyService)
		{
			this.uiService = uiService;
			this.notifyService = notifyService;
		}
		
		public async UniTask ShowAsync(PopupDataModel data)
		{
			this.data = data as InfoPopupDataModel;
			await ShowWindow();
		}

		public async UniTask HideAsync()
		{
			await uiService.Begin<InfoNotificationWindow>()
				.HideAsync();
		}

		private async UniTask ShowWindow()
		{
			await uiService.Begin<InfoNotificationWindow>()
				.WithInitAsync(InitWindow)
				.ShowAsync();

			return;

			Task InitWindow(InfoNotificationWindow window)
			{
				window.SetTitleText(data.TitleText)
					.SetBodyText(data.BodyText)
					.SetConfirmButtonText(data.ConfirmButtonText)
					.SetCancelButtonText(data.CancelButtonText)
					.SetCancelAction(data.OnCancel)
					.SetCloseAction(OnClose)
					.SetConfirmAction(OnClose)
					.SetTopContentVisibility(data.IsTitleVisible)
					.SetMiddleContentVisibility(data.IsDescriptionVisible)
					.SetBottomContentVisibility(data.AreButtonsVisible)
					.SetCancelButtonVisibility(data.AreCancelButtonVisible)
					.SetConfirmButtonVisibility(data.AreConfirmButtonVisible);

				return Task.CompletedTask;
			}
		}

		private void OnClose()
		{
			notifyService.HidePopUpAsync(this);
		}
	}
}