namespace BerserkV3.Common.LiveLinkRouter
{
	public class LiveLinkRouterAdapter
	{
		public static ILiveLinkRouter Service { get; private set; }
		
		public LiveLinkRouterAdapter(ILiveLinkRouter service)
		{
			Service = service;
		}
	}
}