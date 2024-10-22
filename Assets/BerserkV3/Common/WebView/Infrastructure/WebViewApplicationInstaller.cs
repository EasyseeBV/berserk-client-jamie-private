using BerserkV3.Common.WebView.Realizations;
using Zenject;

namespace BerserkV3.Common.WebView.Infrastructure
{
	public class WebViewApplicationInstaller: Installer<WebViewApplicationInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<WebViewApplication>()
				.AsSingle();
		}
	}
}