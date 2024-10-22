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
using RR.UIService;
using Environment = BerserkV3.Startup.Network.Enums.Environment;

namespace BerserkV3.Startup.Authorization
{
	public class AuthSignInState : AuthState<AuthSignInArgs>
	{
		private readonly IUIService uiService;
		private readonly ILiveLinkRouter linkRouter;
		private readonly ISerializeHelper serializeHelper;
		private readonly ISharedConfig sharedConfig;

		public AuthSignInState(
			IUIService uiService,
			ILiveLinkRouter linkRouter,
			ISerializeHelper serializeHelper,
			ISharedConfig sharedConfig)
		{
			this.uiService = uiService;
			this.linkRouter = linkRouter;
			this.serializeHelper = serializeHelper;
			this.sharedConfig = sharedConfig;
		}

		protected override void OnEnter(AuthSignInArgs args)
		{
			uiService.Begin<AuthSignInWindow>()
				.WithInit(InitWindow)
				.Show();
			
			if (args.AutoSignIn && !IsTestingAvailable())
				SignIn().Forget(DefaultSharedLogger.Error);
		}

		protected override void OnExit(AuthSignInArgs args)
		{
			uiService.Begin<AuthSignInWindow>().Hide();
			base.OnExit(args);
		}
		
		private void InitWindow(AuthSignInWindow window)
		{
			var args = GetArgs();
			window.SetHeaderText("Login");
			window.SetEmailText(args.Email);
			window.SetPasswordText(args.Password);
			window.SetTestPasswordText(string.Empty);
			window.SetGuestButtonText("Continue as a guest");
			window.SetSignInButtonText("Login");
			window.SetSignUpButtonText($"Don’t have an account? {"Register".CustomColor("#F55D0D")}");
			window.SetDeleteAccountButtonText("Delete account?");
			window.SetForgotPassButtonText("Forgot Password?");

			if (string.IsNullOrEmpty(args.Email) && serializeHelper.HasKey(SerializeKeyHelper.EMAIL))
				window.SetEmailText(serializeHelper.Get(SerializeKeyHelper.EMAIL));
			
			var socials = sharedConfig.AvailableSocials.Where(x => x.IsPlatformAvailable()).ToArray();
			var isSocialAvailable = socials.IsSocialsAvailable();
			if (isSocialAvailable)
				foreach (var provider in socials)
					SetSocialProvider(provider);
			
			window.SocialWidget.SetActive(isSocialAvailable);
			window.SetActiveSeparator(isSocialAvailable);

			window.SetSignUpAction(StateMachineBus.Switch<AuthSignUpState>);
			window.SetDeleteAccountAction(RedirectToDeleteAccount);
			window.SetForgotPassAction(StateMachineBus.Switch<AuthForgotPassState>);
			window.SetSignInAction(() => SignIn().Forget(DefaultSharedLogger.Error));
			window.SetGuestAction(() => LoginGuest(respose => NotifyAndRetry(respose.GetMessage())).Forget(DefaultSharedLogger.Error));
			window.SetActiveTestPassword(IsTestingAvailable());
				
			return;
			void SetSocialProvider(ExternalProvider provider)
			{
				var button = window.SocialWidget.CreateButton(provider);
				var socialSignInArgs = new AuthSocialSignInArgs {Provider = provider, ReturnState = Id};
				button.Subscribe(() => StateMachineBus.Switch<AuthSocialSignInState>(socialSignInArgs));
			}
		}

		private async UniTask SignIn()
		{
			var window = uiService.Get<AuthSignInWindow>();
			if (IsTestingAvailable())
			{
				var testerResponse = await IdentityAPI.PostLoginInTester(window.GetTestPassword()).AddLoadingTask();
				if (testerResponse.Code != HttpStatusCode.OK)
				{
					NotifyAndRetry($"{testerResponse.GetMessage()}");
					return;
				}
			}

			var authResponse = await IdentityAPI.PostLogIn(window.GetEmail(), window.GetPassword()).AddLoadingTask();
			if (authResponse.Code != HttpStatusCode.OK)
			{
				NotifyAndRetry($"{authResponse.GetMessage()}");
				return;
			}
			
			if (authResponse.GetMessage() == "Redirected:Verification")
			{
				var verificationArgs = new AuthVerificationArgs
				{
					Email = window.GetEmail(), 
					Password = window.GetPassword(), 
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
			var window = uiService.Get<AuthSignInWindow>();
			var args = new AuthSignInArgs {Email = window.GetEmail(), Password = window.GetPassword()};
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