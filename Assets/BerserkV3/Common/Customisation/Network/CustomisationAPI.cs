using System.Collections.Generic;
using System.Threading.Tasks;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using RR.Network.Rest;
using UnityEngine;

namespace BerserkV3.Generic.Customisation
{
	public class CustomisationAPI : API<CustomisationAPI>
	{
		public override string BaseUrl => URLs.APIUrl;

		public override string AuthToken => User.AccessToken ?? "";

		protected override void OnInit()
		{
			OnRequest += Debug.LogWarning;
			OnResponse += Debug.LogWarning;
		}

		public static async Task PostEquippСustomizations(IEnumerable<string> customizationItems)
		{
			await PatchAsync("UICustomization/EquipUserUICustomizations", customizationItems);
		}

		public static async Task<APIResponse<string[]>> GetUserEquippedUICustomizationsIds()
		{
			return await GetAsync<string[]>("UICustomization/GetUserEquippedUICustomizationsIds");
		}
	}
}