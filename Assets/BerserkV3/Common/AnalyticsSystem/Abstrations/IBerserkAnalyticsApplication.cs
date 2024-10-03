using Berserk.Shared.Data.Identity;

namespace BerserkV3.Common.AnalyticsSystem
{

	public interface IBerserkAnalyticsApplication : IAnalyticsApplication
	{
		UserAnalytics AnalyticsData { get; }
	}

}