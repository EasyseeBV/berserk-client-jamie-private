using System;
using BerserkV3.Common.StateMachine;
using RR.Core.DebugSystem;

namespace BerserkV3.Startup.Authorization
{
	public class AuthSocialSignInState : AuthState<AuthSocialSignInArgs>
	{
		private readonly IExternalProviderFactory externalProviderFactory;
		
		public AuthSocialSignInState(IExternalProviderFactory externalProviderFactory)
		{
			this.externalProviderFactory = externalProviderFactory;
		}

		protected override async void OnEnter(AuthSocialSignInArgs args)
		{
			try
			{
				var provider = externalProviderFactory.Create(args.Provider);
				var response = await provider.LoginAsync();
				if (!response.Successful)
				{
					NotifyAndReturn(response.Message);
					return;
				}
				
				if (!response.Model.PreviousLogin.HasValue)
				{
					SuccessfulNotify(() =>
					{
						var processingArgs = new AuthProcessingArgs {AuthModel = response.Model};
						var referralArgs = AuthReferralArgs.Default(nameof(AuthProcessingState), response.Model.Email, response.Model.UserName);
						StateMachineBus.Switch<AuthReferralState>(referralArgs, processingArgs);
					});
					return;
				}

				SuccessfulNotify(() =>
				{
					var processingArgs = new AuthProcessingArgs {AuthModel = response.Model};
					StateMachineBus.Switch<AuthProcessingState>(processingArgs);
				});
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				NotifyAndReturn(e.Message);
			}
		}

		private void NotifyAndReturn(string message)
		{
			var messageArgs = AuthMessageArgs.Retry(GetArgs().ReturnState, message);
			StateMachineBus.Switch<AuthMessageState>(messageArgs);
		}
		
		private void SuccessfulNotify(Action continuation)
		{
			var args = new AuthMessageArgs
			{
				Header = $"{GetArgs().Provider} auth successful!",
				Message = "Continue to complete the login.",
				Success = true,
				ButtonArg = new AuthButtonArg
				{
					Text = "Continue",
					CallBack = () => continuation?.Invoke()
				}
			};
			StateMachineBus.Switch<AuthMessageState>(args);
		}
	}
}