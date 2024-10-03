using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using Berserk.Shared.Data.Identity.Social;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.LiveLinkRouter;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using UnityEngine;

namespace BerserkV3.Startup.Authorization.ExternalProviders
{
	public class ExtenralProviderCommon : ExternalProviderBase, IExternalProvider
	{
		private readonly ExternalProvider provider;

		private UniTaskCompletionSource<ExternalProviderResponse> login;
		private CancellationTokenSource logining;

		private static AuthSocialResponseView Window => AuthSocialResponseView.Instance;

		public ExtenralProviderCommon(ExternalProvider provider)
		{
			this.provider = provider;
		}

		public async UniTask<ExternalProviderResponse> LoginAsync()
		{
			try
			{
				Application.focusChanged += OnApplicationFocusChanged;
				login?.TrySetResult(default);
				login = new UniTaskCompletionSource<ExternalProviderResponse>();
				
				var redirectResponse = await ExternalAPI.PostSocialRedirect(provider).AddLoadingTask();
				if (redirectResponse.Code != HttpStatusCode.OK)
					throw new HttpRequestException(string.Format(RESPONSE_ERROR, provider, redirectResponse.GetMessage()));

				if (string.IsNullOrWhiteSpace(redirectResponse.Data?.RedirectUrl))
					throw new ApplicationException(string.Format(RESPONSE_IS_EMPTY, provider));

				LiveLinkRouterAdapter.Service.OpenLink(redirectResponse.Data.RedirectUrl);

				Window.Show();
				Window.SetHeaderText("Authorization");
				Window.SetMessageText("We are waiting for the provider's response.");
				OnApplicationFocusChanged(true);
				return await login.Task;
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
				Application.focusChanged -= OnApplicationFocusChanged;
				Window.Close();
				logining?.Cancel();
				logining?.Dispose();
				logining = null;
			}
		}

		private void OnApplicationFocusChanged(bool focused)
		{
			if (!focused)
			{
				logining?.Cancel();
				logining?.Dispose();
				logining = null;
				Window.ClearCancelActions();
				return;
			}

			logining = new CancellationTokenSource();
			SocialLoginAsync(logining.Token).Forget();
			Window.SetCancelAction(() => login?.TrySetResult(new ExternalProviderResponse(string.Format(AUTH_CANCELLED, provider), false)));
		}

		private async UniTask SocialLoginAsync(CancellationToken token)
		{
			var providerModel = new AuthLoginProviderModel
			{
				Version = IdentityAPI.GetVersion(),
				DeviceId = SystemInfo.deviceUniqueIdentifier,
				ExternalProvider = provider
			};

			while (true)
			{
				try
				{
					if (!Application.isPlaying || token.IsCancellationRequested)
						break;

					var twoFactorResponce = await IdentityAPI.PostSocialLogin(providerModel);
					if (IsTimeoutResponse(twoFactorResponce))
						throw new TimeoutException(twoFactorResponce.GetMessage());

					if (twoFactorResponce.Code == HttpStatusCode.OK)
					{
						if (twoFactorResponce.Data?.User == null)
							throw new AuthenticationException(string.Format(RESPONSE_IS_EMPTY, provider));

						var response = new ExternalProviderResponse(string.Empty, true)
						{
							Model = twoFactorResponce.Data.User
						};

						login?.TrySetResult(response);
						break;
					}

					if (IsBannedResponse(twoFactorResponce))
					{
						var response400 = new ExternalProviderResponse(twoFactorResponce.RawMessage, false);
						login?.TrySetResult(response400);
						break;
					}

					if (twoFactorResponce.Code != HttpStatusCode.Unauthorized)
						throw new Exception(string.Format(RESPONSE_ERROR, provider, twoFactorResponce.GetMessage()));
					
					await UniTask.Delay(1500, cancellationToken: token);
				}
				catch (OperationCanceledException)
				{
					// Nothing to do
				}
				catch (Exception e) when (e is TimeoutException or AuthenticationException)
				{
					login?.TrySetResult(new ExternalProviderResponse(string.Format(RESPONSE_ERROR, provider, e.Message), false));
				}
				catch (Exception e)
				{
					DefaultSharedLogger.Error(e);
					login?.TrySetResult(new ExternalProviderResponse(string.Format(RESPONSE_ERROR, provider, "Something went wrong. Please make a report and try again."), false));
				}
			}
		}
	}
}