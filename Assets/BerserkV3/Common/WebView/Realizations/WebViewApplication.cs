using System.Threading.Tasks;
using BerserkV3.Common.WebView.Abstractions;
using BerserkV3.Startup.UI;
using RR.Core.DebugSystem;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Common.WebView.Realizations
{
	public class WebViewApplication : IWebViewApplication
	{
		private readonly IUIService uiService;

		public WebViewApplication(IUIService uiService)
		{
			this.uiService = uiService;
		}

		public void OpenLink(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				RRLogger.Error($"{nameof(OpenLink)} failed Can't open empty url.");
				return;
			}

			OpenLinkInWebView(url);
		}
		
		private void OpenLinkInWebView(string url)
		{
			uiService.Begin<WebWindow>()
				.WithInitAsync(InitWindowAsync)
				.ShowAsync();
			
			return;
			async Task InitWindowAsync(WebWindow window)
			{
				await window.SetupAsync(Application.isMobilePlatform, url);
				window.SetCloseAction(() => uiService.Begin<WebWindow>().Hide());
			}
		}
	}
}