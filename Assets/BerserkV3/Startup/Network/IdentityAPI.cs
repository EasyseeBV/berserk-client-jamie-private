using System;
using System.Text;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.Data.Identity.Social;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using Newtonsoft.Json.Linq;
using RR.Core.DebugSystem;
using RR.Network.Rest;
using UnityEngine;

namespace BerserkV3.Startup.Network
{
	public class IdentityAPI : API<IdentityAPI>
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

		public static VersionModel GetVersion()
		{
			return new VersionModel
			{
				Version = Application.version,
				Platform = Application.platform switch
				{
					RuntimePlatform.tvOS => ClientPlatform.IOS,
					RuntimePlatform.IPhonePlayer => ClientPlatform.IOS,
					
					RuntimePlatform.Android => ClientPlatform.Android,
					RuntimePlatform.SamsungTVPlayer => ClientPlatform.Android,
					
					RuntimePlatform.WindowsPlayer => ClientPlatform.Standalone,
					RuntimePlatform.WindowsEditor => ClientPlatform.Standalone,
					RuntimePlatform.OSXEditor => ClientPlatform.Standalone,
					RuntimePlatform.OSXPlayer => ClientPlatform.Standalone,
					RuntimePlatform.LinuxEditor => ClientPlatform.Standalone,
					RuntimePlatform.LinuxPlayer => ClientPlatform.Standalone,
					_ => ClientPlatform.Unknown
				}
			};
		}
		
		public static async Task<APIResponse<string>> PostVerifyVersion()
		{
			return await PostAsync<string>($"Identity/VerifyVersion", GetVersion());
		}
		
		public static async Task<APIResponse<UserDataModel>> GetUserData()
		{
			return await GetAsync<UserDataModel>("Identity/UserData");
		}
		
		public static async Task<APIResponse<ReportThanksModel>> GetReportThanks()
		{
			return await GetAsync<ReportThanksModel>("Identity/ReportThanks");
		}
		
		public static async Task<APIResponse<UserAuthTokensModel>> PostRefreshToken(string token)
		{
			var model = new AuthTokenModel {Version = GetVersion(), Token = token};
			return await PostAsync<UserAuthTokensModel>("Identity/RefreshToken", model);
		}
		
		public static async Task<APIResponse<string>> PostRegister(AuthRegisterModel model)
		{
			return await PostAsync<string>("Identity/Register", model);
		}

		public static async Task<APIResponse<ActivatedAccountModel>> PostVerifyAccount(VerifyAccountModel model)
		{
			return await PostAsync<ActivatedAccountModel>("Identity/VerifyAccount", model);
		}
	
		public static async Task<APIResponse<string>> PostResendVerifyAccount(ResendVerifyAccountModel model)
		{
			return await PostAsync<string>("Identity/ResendVerifyAccount", model);
		}
		
		public static async Task<APIResponse<string>> PostVerifyReferral(VerifyReferralModel model)
		{
			return await PostAsync<string>("Identity/VerifyReferral", model);
		}

		public static async Task<APIResponse<UserAuthModel>> PostLogIn(string email, string password)
		{
			var model = new AuthLoginModel
			{
				Email = email,
				Password = password,
				Version = GetVersion(),
				DeviceId = SystemInfo.deviceUniqueIdentifier
			};

			var response = await PostAsync<TwoFactorModel>("Identity/Login", model);
			return new APIResponse<UserAuthModel>
			{
				Code = response.Code,
				Data = response.Data?.User,
				ErrorMessage = response.ErrorMessage,
				IsSuccess = response.IsSuccess,
				RawMessage = response.RawMessage,
				StatusCodeMessage = response.StatusCodeMessage
			};
		}

		public static async Task<APIResponse<UserAuthModel>> PostLoginGuest()
		{
			var authRequset = new AuthenticateGuestDto
			{
				DeviceId = SystemInfo.deviceUniqueIdentifier,
				Version = Application.version
			};
			
			var response = await PostAsync<TwoFactorModel>("Identity/LoginGuest", authRequset);
			return new APIResponse<UserAuthModel>
			{
				Code = response.Code,
				Data = response.Data?.User,
				ErrorMessage = response.ErrorMessage,
				IsSuccess = response.IsSuccess,
				RawMessage = response.RawMessage,
				StatusCodeMessage = response.StatusCodeMessage
			};
		}

		public static async Task<APIResponse<TwoFactorModel>> PostSocialLogin(AuthLoginProviderModel model)
		{
			return await PostAsync<TwoFactorModel>("Identity/SocialLogin", model);
		}

		public static async Task<APIResponse<string>> PostLoginInTester(string password)
		{
			return await PostAsync<string>($"Identity/LoginInTester?password={password}", jsonContent:null);
		}
		
		public static async Task<APIResponse<UserAuthModel>> PostAuthenticate(string token)
		{
			var model = new AuthTokenModel {Version = GetVersion(), Token = token};
			return (await PostAsync<UserAuthModel>("Identity/Authenticate", model));
		}
		
		public static async Task<APIResponse<string>> PostForgotPassword(ForgotPasswordModel model)
		{
			return await PostAsync<string>("Identity/ForgotPassword", model);
		}
		
		public static async Task<APIResponse<string>> PostResetPassword(ResetPasswordModel model)
		{
			return await PostAsync<string>("Identity/ResetPassword", model);
		}
		
		public static async Task<APIResponse<string>> PostAcceptPrivacyPolicy()
		{
			return await PostAsync<string>("Identity/AcceptedPrivacyPolicy", jsonContent: null);
		}
		
		public static async Task<APIResponse<ApiTimeModel>> GetApiTime()
		{
			return await GetAsync<ApiTimeModel>("Identity/GetApiTime");
		}
	}
}