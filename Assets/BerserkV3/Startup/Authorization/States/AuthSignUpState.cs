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

namespace BerserkV3.Startup.Authorization
{
	public class AuthSignUpState : AuthState<AuthSignUpArgs>
	{
		private readonly ISharedConfig sharedConfig;
		private static AuthSignUpView Window => AuthSignUpView.Instance;

		public AuthSignUpState(ISharedConfig sharedConfig)
		{
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
			
			Window.SetEmailInputText(args.Email);
			Window.SetUserNameInputText(args.UserName);
			Window.SetPasswordInputText(args.Password);
			Window.SetHeaderText("Sign Up");
			Window.SetAuthText("Create you game account!");
			Window.SetGuestButtonText("Continue as a guest");
			Window.SetLoginButtonText($"Already have an account? {"Log in".CustomColor("#F55D0D")}");
			Window.SetContinueButtonText("Continue");
			Window.SetContinueAction(() => RegisterAsync(new AuthRegisterModel
			{
				Email = Window.GetEmail(),
				UserName = Window.GetUserName(),
				Password = Window.GetPassword(),
				Version = IdentityAPI.GetVersion()
			}).Forget());
			Window.SetGuestAction(() => AuthSignInState.LoginGuest(response => NotifyAndRetry(response.GetMessage())).Forget());
			Window.SetLoginAction(StateMachineBus.Switch<AuthSignInState>);

			var socials = sharedConfig.AvailableSocials.Where(x => x.IsPlatformAvailable()).ToArray();
			var isSocialAvailable = socials.IsSocialsAvailable();
			if (isSocialAvailable)
				foreach (var provider in socials)
					SetSocialProvider(provider);
			
			Window.SocialWidget.SetActive(isSocialAvailable);
			Window.SetActiveSeparator(isSocialAvailable);
			Window.Show();
		}
		
		protected override void OnExit(AuthSignUpArgs args)
		{
			Window.Close();
		}
		
		private void SetSocialProvider(ExternalProvider provider)
		{
			var button = Window.SocialWidget.CreateButton(provider);
			var args = new AuthSocialSignInArgs {Provider = provider, ReturnState = Id};
			button.Subscribe(() => StateMachineBus.Switch<AuthSocialSignInState>(args));
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
			var args = new AuthSignUpArgs{Email = Window.GetEmail(), Password = Window.GetPassword(), UserName = Window.GetUserName()};
			StateMachineBus.Switch<AuthMessageState>(AuthMessageArgs.Retry(Id, message), args);
		}
	}
}