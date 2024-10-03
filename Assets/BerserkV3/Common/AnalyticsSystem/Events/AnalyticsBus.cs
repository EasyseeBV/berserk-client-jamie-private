using RR.Core.EventLayer;

namespace BerserkV3.Common.AnalyticsSystem
{

	public class AnalyticsBus : EventBus<AnalyticsBus>
	{
		static AnalyticsBus()
		{
			InitFields<AnalyticsBus>();
		}
		public static RREvent<IAnalyticsModel> SendCustomEvent;
		public static RREvent SendDelayedEvents;
	}

}