using System.Collections.Generic;
using BerserkV3.Startup;
using BerserkV3.Startup.Network.Enums;

namespace BerserkV3.Common.Network // TODO: IMPORTANT do not change, used by reflection in addressable profile
{
	public static class URLs
	{
		public const string API_SUFFIX = "api/v1";

		public static string BlobUrl => GetCurrentBloobUrl(); // TODO: IMPORTANT do not change, used by reflection in addressable profile
		
		public static string HubUrl => $"{ServerUrl}/hubs";

		public static string APIUrl => $"{ServerUrl}/{API_SUFFIX}";
		public static string ShopUrl => $"https://berserk-shop-dev.azurewebsites.net/{API_SUFFIX}"; //TODO Update to dev stage etc

		public static string ServerUrl => URLS_BY_REGION[EnvironmentSwitcher.CurrentRegion].GetServerURL();

		public static readonly Dictionary<Region, URLsList> URLS_BY_REGION = new()
		{
			{
				Region.US, new URLsList(
					"https://prod-berzerk.azurewebsites.net/",
					"https://ccg-berserk-pts.azurewebsites.net/",
					"https://ccg-berzerk-stg.azurewebsites.net/",
					"https://ccg-berzerk-dev.azurewebsites.net/",
					"https://localhost:44394"
				)
			}
		};
		
		public static void SetCustomServerUrl(Region region, string url)
		{
			if (!URLS_BY_REGION.ContainsKey(region))
				URLS_BY_REGION.Add(region, new URLsList());

			URLS_BY_REGION[region].SetCustomUrl(url);
		}
		
		// Dynamic getter when requested then updated
		private static string GetCurrentBloobUrl()
		{
			return new URLsList(
				$"https://vulcanversestorage.blob.core.windows.net/server-core/{EnvironmentSwitcher.CurrentEnvironment}",
				$"https://berzerkfunctionsdev.blob.core.windows.net/server-core/{EnvironmentSwitcher.CurrentEnvironment}",
				$"https://berzerkfunctionsdev.blob.core.windows.net/server-core/{EnvironmentSwitcher.CurrentEnvironment}",
				$"https://berzerkfunctionsdev.blob.core.windows.net/server-core/{EnvironmentSwitcher.CurrentEnvironment}",
				$"https://berzerkfunctionsdev.blob.core.windows.net/server-core/{Environment.Development}", // Default Dev
				$"https://berzerkfunctionsdev.blob.core.windows.net/server-core/{Environment.Custom}"
			).GetServerURL();
		}
	}
}