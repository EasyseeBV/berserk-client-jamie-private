using BerserkV3.Startup;
using BerserkV3.Startup.Network.Enums;

namespace BerserkV3.Common.Network
{
	public class URLsList
	{
		private readonly string productionServerURL;
		private readonly string publicTestServerURL;
		private readonly string stagingServerURL;
		private readonly string developmentServerURL;
		private readonly string localhostServerURL;
		private string customServerURL;
		
		public URLsList(
			string productionURL = null, 
			string publicTestURL = null, 
			string stagingURL = null, 
			string developmentURL = null,
			string localhostURL = null, 
			string customServerURL = null)
		{
			productionServerURL = productionURL;
			publicTestServerURL = publicTestURL;
			stagingServerURL = stagingURL;
			developmentServerURL = developmentURL;
			localhostServerURL = localhostURL;
			this.customServerURL = customServerURL;
		}

		public void SetCustomUrl(string url)
		{
			customServerURL = url;
		}
		
		public string GetServerURL()
		{
			return GetServerURL(EnvironmentSwitcher.CurrentEnvironment);
		}
		
		public string GetServerURL(Environment environment)
		{
			return environment switch
			{
				Environment.Production => productionServerURL,
				Environment.Test => publicTestServerURL,
				Environment.Staging => stagingServerURL,
				Environment.Development => developmentServerURL,
				Environment.LocalHost => localhostServerURL,
				Environment.Custom => customServerURL,
				_ => stagingServerURL
			};
		}
	}
}