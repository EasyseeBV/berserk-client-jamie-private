using System.Collections.Generic;
using RR.Core.Extensions;

namespace BerserkV3.Common.AnalyticsSystem
{

	public class CustomModel : AnalyticsModel
	{
		private readonly Dictionary<string, string> parameters;

		public CustomModel(string eventName, Dictionary<string, string> parameters)
		{
			this.parameters = parameters;
			Key = eventName;
		}

		protected override void FillData()
		{
			base.FillData();
			parameters.ForEach(kpv => AddModelParameter(kpv.Key, kpv.Value));
		}
	}

}