using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Startup.UI;
using RR.Core.DebugSystem;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Common.LiveLinkRouter
{
	public class LiveLinkRouterService : ILiveLinkRouter
	{
		private readonly IGameDatabase gameDatabase;
		private readonly IUIService uiService;
		public LiveLinkRouterService(IGameDatabase gameDatabase, IUIService uiService)
		{
			this.gameDatabase = gameDatabase;
			this.uiService = uiService;
		}

		public void OpenLink(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				RRLogger.Error($"Can't open empty url : {url}");
				return;
			}

			OpenLinkInWebView(url);
		}

		public void OpenLinkByKey(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				RRLogger.Error($"Can't open empty url : {key}");
				return;
			}
			
			OpenLinkInWebView(gameDatabase.GetLink(key)?.Link);
		}

		private void OpenLinkInWebView(string url)
		{
			uiService.Begin<WebWindow>()
				.WithInitAsync(InitWindowAsync)
				.Show();
			
			return;
			async Task InitWindowAsync(WebWindow window)
			{
				await window.SetupAsync(Application.isMobilePlatform, url);
				window.SetCloseAction(() => uiService.Begin<WebWindow>().Hide());
			}
		}
	}
}