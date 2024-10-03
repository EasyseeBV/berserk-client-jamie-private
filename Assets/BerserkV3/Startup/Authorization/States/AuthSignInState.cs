using System;
using System.Linq;
using System.Net;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.Data.Identity.Social;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.LiveLinkRouter;
using BerserkV3.Common.Network;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Events;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using BerserkV3.Startup.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Network.Rest;
using Environment = BerserkV3.Startup.Network.Enums.Environment;

namespace BerserkV3.Startup.Authorization
{
	public class AuthSignInState : AuthState<AuthSignInArgs>
	{
		private readonly ILiveLinkRouter linkRouter;
		private readonly ISerializeHelper serializeHelper;
		private readonly ISharedConfig sharedConfig;
		
		private static AuthSignInView Window => AuthSignInView.Instance;

		public AuthSignInState(
			ILiveLinkRouter linkRouter,
			ISerializeHelper serializeHelper,
			ISharedConfig sharedConfig)
		{
			this.linkRouter = linkRouter;
			this.serializeHelper = serializeHelper;
			this.sharedConfig = sharedConfig;
		}

		protected override void OnEnter(AuthSignInArgs args)
		{
			Window.SetHeaderText("Login");
			Window.SetEmailText(args.Email);
			Window.SetPasswordText(args.Password);
			Window.SetTestPasswordText(string.Empty);
			Window.SetGuestButtonText("Continue as a guest");
			Window.SetSignInButtonText("Login");
			Window.SetSignUpButtonText($"Don’t have an account? {"Register".CustomColor("#F55D0D")}");
			Window.SetDeleteAccountButtonText("Delete account?");
			Window.SetForgotPassButtonText("Forgot Password?");

			if (string.IsNullOrEmpty(args.Email) && serializeHelper.HasKey(SerializeKeyHelper.EMAIL))
				Window.SetEmailText(serializeHelper.Get(SerializeKeyHelper.EMAIL));
			
			var socials = sharedConfig.AvailableSocials.Where(x => x.IsPlatformAvailable()).ToArray();
			var isSocialAvailable = socials.IsSocialsAvailable();
			if (isSocialAvailable)
				foreach (var provider in socials)
					SetSocialProvider(provider);
			
			Window.SocialWidget.SetActive(isSocialAvailable);
			Window.SetActiveSeparator(isSocialAvailable);

			Window.SetSignUpAction(StateMachineBus.Switch<AuthSignUpState>);
			Window.SetDeleteAccountAction(RedirectToDeleteAccount);
			Window.SetForgotPassAction(StateMachineBus.Switch<AuthForgotPassState>);
			Window.SetSignInAction(() => SignIn().Forget(DefaultSharedLogger.Error));
			Window.SetGuestAction(() => LoginGuest(respose => NotifyAndRetry(respose.GetMessage())).Forget(DefaultSharedLogger.Error));
			Window.SetActiveTestPassword(IsTestingAvailable());
			Window.Show();
			
			if (args.AutoSignIn && !IsTestingAvailable())
				SignIn().Forget(DefaultSharedLogger.Error);
		}

		protected override void OnExit(AuthSignInArgs args)
		{
			base.OnExit(args);
			Window.Close();
		}

		private void SetSocialProvider(ExternalProvider provider)
		{
			var button = Window.SocialWidget.CreateButton(provider);
			var args = new AuthSocialSignInArgs {Provider = provider, ReturnState = Id};
			button.Subscribe(() => StateMachineBus.Switch<AuthSocialSignInState>(args));
		}

		private async UniTask SignIn()
		{
			if (IsTestingAvailable())
			{
				var testerResponse = await IdentityAPI.PostLoginInTester(Window.GetTestPassword()).AddLoadingTask();
				if (testerResponse.Code != HttpStatusCode.OK)
				{
					NotifyAndRetry($"{testerResponse.GetMessage()}");
					return;
				}
			}

			var authResponse = await IdentityAPI.PostLogIn(Window.GetEmail(), Window.GetPassword()).AddLoadingTask();
			if (authResponse.Code != HttpStatusCode.OK)
			{
				NotifyAndRetry($"{authResponse.GetMessage()}");
				return;
			}
			
			if (authResponse.GetMessage() == "Redirected:Verification")
			{
				var verificationArgs = new AuthVerificationArgs
				{
					Email = Window.GetEmail(), 
					Password = Window.GetPassword(), 
					UserName = ""
				};
				StateMachineBus.Switch<AuthVerificationState>(verificationArgs);
				return;
			}
			
			var processingArgs = new AuthProcessingArgs {AuthModel = authResponse.Data};
			StateMachineBus.Switch<AuthProcessingState>(processingArgs);
		}		
		
		public static async UniTask LoginGuest(Action<APIResponse<UserAuthModel>> fallBack)
		{
			var response = await IdentityAPI.PostLoginGuest().AddLoadingTask();
			if (response.Code != HttpStatusCode.OK)
			{
				fallBack?.Invoke(response);
				return;
			}

			var processingArgs = new AuthProcessingArgs {AuthModel = response.Data};
			StateMachineBus.Switch<AuthProcessingState>(processingArgs);
		}

		private void NotifyAndRetry(string message)
		{
			var args = new AuthSignInArgs {Email = Window.GetEmail(), Password = Window.GetPassword()};
			StateMachineBus.Switch<AuthMessageState>(AuthMessageArgs.Retry(Id, message), args);
		}

		private void RedirectToDeleteAccount()
		{
			linkRouter.OpenLinkByKey(LinkKeyHelper.DELETE_ACCOUNT);
		}

		private static bool IsTestingAvailable()
		{
			return !StartupBus.TestFly && EnvironmentSwitcher.CurrentEnvironment is Environment.Test;
		}
	}
}