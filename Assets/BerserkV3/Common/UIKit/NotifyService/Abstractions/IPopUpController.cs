using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.UIKit.Abstractions
{
	public interface IPopUpController
	{
		public PopupDataModel Data { get; }
		UniTask ShowAsync(PopupDataModel data);
		UniTask HideAsync(); //async opening
	}
}