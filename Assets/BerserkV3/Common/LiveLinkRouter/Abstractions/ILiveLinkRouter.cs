namespace BerserkV3.Common.LiveLinkRouter
{
	public interface ILiveLinkRouter
	{
		void OpenLink(string url);
		void OpenLinkByKey(string key);
	}
}