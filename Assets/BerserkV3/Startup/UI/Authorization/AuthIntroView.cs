using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace BerserkV3.Startup.UI
{
	public partial class AuthIntroView : SafeView
	{
		public async UniTask ShowAsync(float duration)
		{
			Show();
			await LogoImage.DOFade(0, 0).ToUniTask();
			await LogoImage.DOFade(1, duration).ToUniTask();
		}

		public async UniTask CloseAsync(float duration)
		{
			await LogoImage.DOFade(1, 0).ToUniTask();
			await LogoImage.DOFade(0, duration).ToUniTask();
			Close();
		}
	}
}