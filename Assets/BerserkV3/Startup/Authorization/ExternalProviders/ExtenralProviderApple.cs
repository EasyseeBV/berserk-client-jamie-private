using System;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.Data.Identity.Social;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Network;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Startup.Authorization.ExternalProviders
{
	public class ExtenralProviderApple : ExternalProviderBase, IExternalProvider
	{
		private readonly IUIService uiService;
		private readonly ExternalProvider provider;
		private readonly IGameDatabase gameDatabase;
		private readonly ISerializeHelper serializeHelper;

		public ExtenralProviderApple(
			IUIService uiService,
			ExternalProvider provider,
			IGameDatabase gameDatabase,
			ISerializeHelper serializeHelper)
		{
			this.uiService = uiService;
			this.provider = provider;
			this.gameDatabase = gameDatabase;
			this.serializeHelper = serializeHelper;
		}

		public async UniTask<ExternalProviderResponse> LoginAsync()
		{
			var internalTaskSource = new CancellationTokenSource();
			var internalToken = internalTaskSource.Token;
			try
			{
				if (!AppleAuthManager.IsCurrentPlatformSupported)
					throw new Exception("Current platform is unsupported");

				uiService.Begin<AuthSocialResponseWindow>()
					.WithInit(window =>
					{
						window.ClearCancelActions();
						window.SetHeaderText("Authorization");
						window.SetMessageText("We are waiting for the provider's response.");
						window.SetCancelAction(() => internalTaskSource?.Cancel());
					})
					.Show();

				var deserializer = new PayloadDeserializer();
				var appleAuthManager = new AppleAuthManager(deserializer);
				RefreshManagerTaskAsync(appleAuthManager, internalToken).Forget();

				var loginResponse = await QuickLoginAsync(appleAuthManager, internalToken);
				if (!loginResponse.Successful)
					loginResponse = await FullLoginAsync(appleAuthManager, internalToken);

				if (!loginResponse.Successful)
					return loginResponse;

				return await SocialLoginAsync(loginResponse, internalToken);
			}
			catch (OperationCanceledException)
			{
				return new ExternalProviderResponse(string.Format(AUTH_CANCELLED, provider), false);
			}
			catch (AuthenticationException e)
			{
				return new ExternalProviderResponse(e.Message, false);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return new ExternalProviderResponse(e.Message, false);
			}
			finally
			{
				uiService.Begin<AuthSocialResponseWindow>().Hide();
				internalTaskSource.Cancel();
				internalTaskSource.Dispose();
				internalTaskSource = null;
			}
		}

		private ExternalProviderResponse GetResponseFromCredential(ICredential credentials)
		{
			if (credentials.User == null)
				return new ExternalProviderResponse($"{nameof(credentials.User)} is missing.", false);

			if (credentials is not IAppleIDCredential appleIdCredential)
				return new ExternalProviderResponse($"Unknown {nameof(credentials)} : {credentials.GetType().Name}", false);

			if (!serializeHelper.HasKey(SerializeKeyHelper.USERID)
			    || serializeHelper.Get(SerializeKeyHelper.USERID) != credentials.User)
			{
				serializeHelper.Patch(SerializeKeyHelper.USERID, credentials.User);
				serializeHelper.Save();
			}

			return new ExternalProviderResponse(string.Empty, true)
			{
				Code = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode, 0, appleIdCredential.AuthorizationCode.Length),
				Model = new UserAuthModel
				{
					Email = appleIdCredential.Email,
					UserName = appleIdCredential.FullName?.Nickname,
					AccessToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length),
					Id = appleIdCredential.User
				}
			};
		}

		private async UniTask<ExternalProviderResponse> FullLoginAsync(IAppleAuthManager manager, CancellationToken token)
		{
			var loginSource = new UniTaskCompletionSource<ExternalProviderResponse>();
			var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
			manager.LoginWithAppleId(loginArgs,
				credentials => loginSource.TrySetResult(GetResponseFromCredential(credentials)),
				error => loginSource.TrySetResult(new ExternalProviderResponse(FormatErrorResponse(error), false)));

			return await loginSource.Task.AttachExternalCancellation(token);
		}

		private async UniTask<ExternalProviderResponse> QuickLoginAsync(IAppleAuthManager manager, CancellationToken token)
		{
			if (!serializeHelper.HasKey(SerializeKeyHelper.USERID))
				return new ExternalProviderResponse("Not Authorized.", false);

			var checkAuthStatus = new UniTaskCompletionSource<bool>();
			manager.GetCredentialState(serializeHelper.Get(SerializeKeyHelper.USERID),
				state => checkAuthStatus.TrySetResult(state == CredentialState.Authorized),
				_ => checkAuthStatus.TrySetResult(false));

			if (!await checkAuthStatus.Task.AttachExternalCancellation(token))
				return new ExternalProviderResponse("Not Authorized.", false);

			var quickLoginArgs = new AppleAuthQuickLoginArgs();
			var loginSource = new UniTaskCompletionSource<ExternalProviderResponse>();
			manager.QuickLogin(quickLoginArgs,
				credentials => loginSource.TrySetResult(GetResponseFromCredential(credentials)),
				error => loginSource.TrySetResult(new ExternalProviderResponse(FormatErrorResponse(error), false)));

			return await loginSource.Task.AttachExternalCancellation(token);
		}

		private string FormatErrorResponse(IAppleError error)
		{
			return error.Code is 1000 or 1001
				? gameDatabase.GetLocalization("ClientAuth_AppleFailed_NoConnection")
				: error.LocalizedDescription;
		}

		private async UniTask RefreshManagerTaskAsync(IAppleAuthManager manager, CancellationToken token)
		{
			try
			{
				while (Application.isPlaying && !token.IsCancellationRequested)
				{
					manager.Update();
					await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, token);
				}
			}
			catch
			{
				// Nothing to do
			}
		}

		private async UniTask<ExternalProviderResponse> SocialLoginAsync(ExternalProviderResponse loginResponse, CancellationToken token)
		{
			try
			{
				var socialAuthResponse = await ExternalAPI.GetAppleAuthorize(loginResponse.Code)
					.AsUniTask().AttachExternalCancellation(token);

				if (IsTimeoutResponse(socialAuthResponse))
					throw new TimeoutException(socialAuthResponse.GetMessage());

				if (IsBannedResponse(socialAuthResponse))
					return new ExternalProviderResponse(socialAuthResponse.RawMessage, false);

				if (socialAuthResponse.Code != HttpStatusCode.OK)
					throw new Exception(string.Format(RESPONSE_ERROR, provider, socialAuthResponse.GetMessage()));

				if (socialAuthResponse.Data == null)
					throw new AuthenticationException(string.Format(RESPONSE_IS_EMPTY, provider));

				var providerModel = socialAuthResponse.Data;
				providerModel.DeviceId = SystemInfo.deviceUniqueIdentifier;
				providerModel.ExternalProvider = provider;
				providerModel.Version = IdentityAPI.GetVersion();
				
				var twoFactorResponce = await IdentityAPI.PostSocialLogin(providerModel)
					.AsUniTask().AttachExternalCancellation(token);

				if (IsTimeoutResponse(twoFactorResponce))
					throw new TimeoutException(twoFactorResponce.GetMessage());

				if (IsBannedResponse(twoFactorResponce))
					return new ExternalProviderResponse(twoFactorResponce.RawMessage, false);

				if (twoFactorResponce.Code != HttpStatusCode.OK)
					throw new Exception(string.Format(RESPONSE_ERROR, provider, twoFactorResponce.GetMessage()));

				if (twoFactorResponce.Data?.User == null)
					throw new AuthenticationException(string.Format(RESPONSE_IS_EMPTY, provider));

				return new ExternalProviderResponse(string.Empty, true)
				{
					Model = twoFactorResponce.Data.User
				};
			}
			catch (OperationCanceledException)
			{
				return new ExternalProviderResponse(string.Format(AUTH_CANCELLED, provider), false);
			}
			catch (Exception e) when (e is TimeoutException or AuthenticationException)
			{
				return new ExternalProviderResponse(string.Format(RESPONSE_ERROR, provider, e.Message), false);
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return new ExternalProviderResponse(string.Format(RESPONSE_ERROR, provider, "Something went wrong. Please make a report and try again."), false);
			}
		}
	}
}