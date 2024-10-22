using System.Linq;
using System.Net;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.Data.Identity.Social;
using BerserkV3.Common.Network;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using BerserkV3.Startup.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.UIService;

namespace BerserkV3.Startup.Authorization
{
	public class AuthSignUpState : AuthState<AuthSignUpArgs>
	{
		private readonly IUIService uiService;
		private readonly ISharedConfig sharedConfig;

		public AuthSignUpState(
			IUIService uiService, 
			ISharedConfig sharedConfig)
		{
			this.uiService = uiService;
			this.sharedConfig = sharedConfig;
		}

		protected override void OnEnter(AuthSignUpArgs args)
		{
			if (args.AutoSignUp)
			{
				RegisterAsync(new AuthRegisterModel
				{
					Email = args.Email,
					UserName = args.UserName,
					Password = args.Password,
					Version = IdentityAPI.GetVersion()
				}).Forget();
				return;
			}
			
			uiService.Begin<AuthSignUpWindow>()
				.WithInit(InitWindow)
				.Show();

			
			return;

			void InitWindow(AuthSignUpWindow window)
			{
							
				window.SetEmailInputText(args.Email);
				window.SetUserNameInputText(args.UserName);
				window.SetPasswordInputText(args.Password);
				window.SetHeaderText("Sign Up");
				window.SetAuthText("Create you game account!");
				window.SetGuestButtonText("Continue as a guest");
				window.SetLoginButtonText($"Already have an account? {"Log in".CustomColor("#F55D0D")}");
				window.SetContinueButtonText("Continue");
				window.SetContinueAction(() => RegisterAsync(new AuthRegisterModel
				{
					Email = window.GetEmail(),
					UserName = window.GetUserName(),
					Password = window.GetPassword(),
					Version = IdentityAPI.GetVersion()
				}).Forget());
				window.SetGuestAction(() => AuthSignInState.LoginGuest(response => NotifyAndRetry(response.GetMessage())).Forget());
				window.SetLoginAction(StateMachineBus.Switch<AuthSignInState>);

				var socials = sharedConfig.AvailableSocials.Where(x => x.IsPlatformAvailable()).ToArray();
				var isSocialAvailable = socials.IsSocialsAvailable();
				if (isSocialAvailable)
					foreach (var provider in socials)
						SetSocialProvider(provider);
			
				window.SocialWidget.SetActive(isSocialAvailable);
				window.SetActiveSeparator(isSocialAvailable);
				
				return;
				void SetSocialProvider(ExternalProvider provider)
				{
					var button = window.SocialWidget.CreateButton(provider);
					var socialSignInArgs = new AuthSocialSignInArgs {Provider = provider, ReturnState = Id};
					button.Subscribe(() => StateMachineBus.Switch<AuthSocialSignInState>(socialSignInArgs));
				}
			}
		}
		
		protected override void OnExit(AuthSignUpArgs args)
		{
			uiService.Begin<AuthSignUpWindow>().Hide();
		}
		
		private async UniTask RegisterAsync(AuthRegisterModel model)
		{
			var response = await IdentityAPI.PostRegister(model).AddLoadingTask();
			if (response.Code != HttpStatusCode.OK)
			{
				NotifyAndRetry(response.GetMessage());
				return;
			}
			
			var referralArgs = AuthReferralArgs.Default(nameof(AuthVerificationState), model.Email, model.UserName);
			var verificationArgs = new AuthVerificationArgs(model);
			StateMachineBus.Switch<AuthReferralState>(referralArgs, verificationArgs);
		}

		private void NotifyAndRetry(string message)
		{
			var window = uiService.Get<AuthSignUpWindow>();
			var args = new AuthSignUpArgs{Email = window.GetEmail(), Password = window.GetPassword(), UserName = window.GetUserName()};
			StateMachineBus.Switch<AuthMessageState>(AuthMessageArgs.Retry(Id, message), args);
		}
	}
}