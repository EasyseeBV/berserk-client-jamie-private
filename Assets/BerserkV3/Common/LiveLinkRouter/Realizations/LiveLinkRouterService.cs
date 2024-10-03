using Berserk.Shared.Data.Abstraction;
using RR.Core.DebugSystem;
using UnityEngine;

namespace BerserkV3.Common.LiveLinkRouter
{
	public class LiveLinkRouterService : ILiveLinkRouter
	{
		private readonly IGameDatabase gameDatabase;
		public LiveLinkRouterService(IGameDatabase gameDatabase)
		{
			this.gameDatabase = gameDatabase;
		}

		public void OpenLink(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				RRLogger.Error($"Can't open empty url : {url}");
				return;
			}
			
#if !UNITY_EDITOR && UNITY_IOS
			SfSafariView.OpenURL(url);
#else
			Application.OpenURL(url);
#endif

		}

		public void OpenLinkByKey(string key)
		{
			OpenLink(gameDatabase.GetLink(key)?.Link);
		}
	}
}