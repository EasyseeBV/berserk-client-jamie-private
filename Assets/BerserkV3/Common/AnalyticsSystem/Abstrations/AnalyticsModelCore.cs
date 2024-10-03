using System.Collections.Generic;

namespace BerserkV3.Common.AnalyticsSystem
{
	public class AnalyticsModelCore : IAnalyticsModel
	{
		private readonly bool isAdmin;
		public string Key { get; }

		public AnalyticsModelCore()
		{
#if !PRODUCTION
			isAdmin = true;
#else
			isAdmin = false;
#endif
		}

		public Dictionary<string, object> ToDictionary()
		{
			return new Dictionary<string, object>
			{
				[nameof(isAdmin)] = isAdmin
			};
		}
	}
}