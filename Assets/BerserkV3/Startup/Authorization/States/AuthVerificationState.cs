using System.Net;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Network;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Startup.Authorization
{
	public class AuthVerificationState : AuthState<AuthVerificationArgs>
	{
		private readonly IUIService uiService;
		private const float DELAY_RESET_REQEST = 30;
		
		public AuthVerificationState(IUIService uiService)
		{
			this.uiService = uiService;
		}

		protected override void OnEnter(AuthVerificationArgs args)
		{
			uiService.Begin<AuthVerificationWindow>()
				.WithInit(InitWindow)
				.Show();
			
			DelayBewenReqest();
			return;

			void InitWindow(AuthVerificationWindow window)
			{
				window.SetInputText(string.Empty);
				window.SetSubmitText("Continue");
				window.SetHeaderText("Verify Your Email");
				window.SetFooterText("Didn't receive an email? <color=#F55D0D>Send Again");
				window.SetMessageText("We have sent you an email with verification link,<br>" +
				                      "please click on that link to proceed further.");
			
				window.SetInputChangedAction(v => window.SetSubmitInteractable(!string.IsNullOrEmpty(v)));
				window.SetSubminAction(() => SubmitVerificationAsync().Forget(DefaultSharedLogger.Error));
				window.SetFooterAction(() => ResendVerificationMessageAsync().Forget(DefaultSharedLogger.Error));
				window.SetReturnAction(StateMachineBus.Switch<AuthSignUpState>);
				window.SetSubmitInteractable(false);
			}
		}

		protected override void OnExit(AuthVerificationArgs args)
		{
			uiService.Begin<AuthVerificationWindow>().Hide();
		}

		private async UniTask ResendVerificationMessageAsync()
		{
			var stateArgs = GetArgs();
			var verifyAccount = new ResendVerifyAccountModel
			{
				Email = stateArgs.Email
			};
			
			var response = await IdentityAPI.PostResendVerifyAccount(verifyAccount).AddLoadingTask();
			if (response.Code != HttpStatusCode.OK)
			{
				NotifyAndRetry(response.GetMessage());
				return;
			}
			
			DelayBewenReqest();
		}

		private async UniTask SubmitVerificationAsync()
		{
			var window = uiService.Get<AuthVerificationWindow>();
			var stateArgs = GetArgs();
			var verifyAccount = new VerifyAccountModel
			{
				ActivationCode = window.GetInput(),
				Email = stateArgs.Email,
				UserName = stateArgs.UserName
			};
			
			var response = await IdentityAPI.PostVerifyAccount(verifyAccount).AddLoadingTask();
			if (response.Code != HttpStatusCode.OK || response.Data == null)
			{
				NotifyAndRetry(response.GetMessage());
				return;
			}

			if (!response.Data.Status)
			{
				var retryArgs = AuthMessageArgs.Retry(nameof(AuthVerificationArgs), response.Data.Message);
				StateMachineBus.Switch<AuthMessageState>(retryArgs, GetArgs());
				return;
			}
			
			var message = "The account was successfully activated.";
			var acceptedArgs = AuthMessageArgs.Accepted(nameof(AuthSignInState), message);
			var signInArgs = new AuthSignInArgs {Email = stateArgs.Email, Password = stateArgs.Password, AutoSignIn = true};
			StateMachineBus.Switch<AuthMessageState>(acceptedArgs, signInArgs);
		}

		private void NotifyAndRetry(string message)
		{
			AddStateArgs(AuthMessageArgs.Retry(Id, message));
			StateMachineBus.Switch<AuthMessageState>(GetRawArgs());
		}

		private void DelayBewenReqest()
		{
			if (!Application.isPlaying)
				return;

			var window = uiService.Get<AuthVerificationWindow>();
			window.SetFooterInteractable(false);
			window.TimerWidget.SetTimer(DELAY_RESET_REQEST, () =>
			{
				window.TimerWidget.SetActive(false);
				window.SetFooterInteractable(true);
			});
		}
	}
}