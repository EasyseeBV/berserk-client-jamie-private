using System;
using System.Text;
using System.Threading.Tasks;
using Berserk.Shared.Data.Identity.Social;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using Newtonsoft.Json.Linq;
using RR.Core.DebugSystem;
using RR.Network.Rest;
using UnityEngine.Device;

namespace BerserkV3.Startup.Network
{
	public class ExternalAPI : API<IdentityAPI>
	{
		public override string BaseUrl => URLs.APIUrl;
		public override string AuthToken => User.AccessToken;

		private string[] SecureSensitiveWords { get; } =
		{
			"Password",
			"Email",
			"UserName",
			"Secret",
			"AccessToken",
			"RefreshToken",
			"Token",
			"PinCode"
		};

		protected override void OnInit()
		{
			OnRequest += WriteLogSecure;
			OnResponse += WriteLogSecure;
		}

		private void WriteLogSecure(string message)
		{
			message = ReplaceSecureData(message);
			RRLogger.Warning(message);
		}

		private string ReplaceSecureData(string data)
		{
			if (string.IsNullOrEmpty(data) || data is "null" or "{}")
				return data;

			var entry = data.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				var result = new StringBuilder();
				// try parse json and format secure data
				foreach (var item in entry)
				{
					try
					{
						var jObject = JObject.Parse(item);
						foreach (var path in SecureSensitiveWords)
						{
							jObject.SelectToken(path, false)?.Replace("*****");
						}

						// append result, remove pretty print format
						result.AppendLine(jObject.ToString().Replace("\n", ""));
					}
					catch (Exception)
					{
						// if any exeption need include data at result
						result.AppendLine(item);
					}
				}

				// if format will not work return source data
				return result.Length == 0 ? data : result.ToString();
			}
			catch
			{
				// if any exeption, return source data
				return data;
			}
		}
		

		public static async Task<APIResponse<SocialRedirectModel>> PostSocialRedirect(ExternalProvider provider)
		{
			return await PostAsync<SocialRedirectModel>($"External/SocialRedirect", new SocialRequestRedirectModel
			{
				ExternalProvider = provider,
				DeviceId = SystemInfo.deviceUniqueIdentifier,
			});
		}

		public static async Task<APIResponse<AuthLoginProviderModel>> GetAppleAuthorize(string code)
		{
			return await GetAsync<AuthLoginProviderModel>($"External/AppleAuthorize?code={code}&state={SystemInfo.deviceUniqueIdentifier}");
		}
	}
}