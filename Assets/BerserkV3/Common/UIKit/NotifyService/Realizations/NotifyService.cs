using System.Collections.Generic;
using BerserkV3.Common.UIKit.Abstractions;
using Cysharp.Threading.Tasks;
using Zenject;

namespace BerserkV3.Common.UIKit.NotifyService.Realizations
{
	public class NotifyService : INotifyService
	{
		private readonly LinkedList<IPopUpController> popUpQueue = new LinkedList<IPopUpController>();
		private readonly IInstantiator instantiator;
		
		private bool isShowingPopUp;
		private IPopUpController currentController;
		
		public NotifyService(IInstantiator instantiator)
		{
			this.instantiator = instantiator;
		}

		public async UniTask ShowPopUpAsync<T>(PopupDataModel data) where T : IPopUpController
		{
			var controller = CreateController<T>();

			if (!isShowingPopUp)
			{
				isShowingPopUp = true;
				currentController = controller;
				await ShowAndHandlePopUpAsync(controller, data);
			}
			else
			{
				popUpQueue.AddLast(controller);
			}
		}

		public async UniTask HidePopUpAsync(IPopUpController controller)
		{
			if (controller == currentController)
			{
				await controller.HideAsync();

				currentController = null;
				
				if (popUpQueue.Count > 0)
				{
					var nextNode = popUpQueue.First;
					popUpQueue.RemoveFirst();
					var nextController = nextNode.Value;
					currentController = nextController;
					var data = nextController.Data;
					await ShowAndHandlePopUpAsync(nextController, data);
				}
				else
				{
					isShowingPopUp = false;
				}
			}
		}

		public async UniTask RemoveControllerAsync(IPopUpController controller)
		{
			if (controller == currentController && isShowingPopUp)
			{
				await HidePopUpAsync(controller);
			}
			else
			{
				var node = popUpQueue.Find(controller);
				if (node != null)
				{
					popUpQueue.Remove(node);
				}
			}
		}

		private async UniTask ShowAndHandlePopUpAsync(IPopUpController controller, PopupDataModel data)
		{
			await controller.ShowAsync(data);
		}

		private IPopUpController CreateController<T>() where T : IPopUpController
		{
			var controller = instantiator.Instantiate<T>();
			return controller;
		}
	}
}