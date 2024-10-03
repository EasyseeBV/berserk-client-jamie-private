using System.Net;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Network;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.Startup.Authorization
{
	public class AuthVerificationState : AuthState<AuthVerificationArgs>
	{
		private const float DELAY_RESET_REQEST = 30;
		private static AuthVerificationView Window => AuthVerificationView.Instance;

		protected override void OnEnter(AuthVerificationArgs args)
		{
			Window.SetInputText(string.Empty);
			Window.SetSubmitText("Continue");
			Window.SetHeaderText("Verify Your Email");
			Window.SetFooterText("Didn't receive an email? <color=#F55D0D>Send Again");
			Window.SetMessageText("We have sent you an email with verification link,<br>" +
			                      "please click on that link to proceed further.");
			
			Window.SetInputChangedAction(v => Window.SetSubmitInteractable(!string.IsNullOrEmpty(v)));
			Window.SetSubminAction(() => SubmitVerificationAsync().Forget(DefaultSharedLogger.Error));
			Window.SetFooterAction(() => ResendVerificationMessageAsync().Forget(DefaultSharedLogger.Error));
			Window.SetReturnAction(StateMachineBus.Switch<AuthSignUpState>);
			Window.SetSubmitInteractable(false);
			Window.Show();
			
			DelayBewenReqest();
		}

		protected override void OnExit(AuthVerificationArgs args)
		{
			Window.Close();
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
			var stateArgs = GetArgs();
			var verifyAccount = new VerifyAccountModel
			{
				ActivationCode = Window.GetInput(),
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
			
			Window.SetFooterInteractable(false);
			Window.TimerWidget.SetTimer(DELAY_RESET_REQEST, () =>
			{
				Window.TimerWidget.SetActive(false);
				Window.SetFooterInteractable(true);
			});
		}
	}
}