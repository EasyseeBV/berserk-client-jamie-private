using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.UIKit.Abstractions
{
	public interface INotifyService
	{
		UniTask ShowPopUpAsync<T>(PopupDataModel data) where T : IPopUpController;
		UniTask HidePopUpAsync(IPopUpController controller);
		UniTask RemoveControllerAsync(IPopUpController controller); // Call it from PopupDataModel.OnRelease?.Invoke
	}
}