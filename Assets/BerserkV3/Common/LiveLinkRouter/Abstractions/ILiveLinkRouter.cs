using System;
using Vuplex.WebView;

namespace BerserkV3.Common.LiveLinkRouter
{
	public interface ILiveLinkRouter
	{
		public void OpenLink(string url);
		void OpenLinkByKey(string key);
	}
}