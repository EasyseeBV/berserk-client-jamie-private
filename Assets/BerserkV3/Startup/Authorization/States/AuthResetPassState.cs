using System.Net;
using Berserk.Shared.Data.Identity;
using BerserkV3.Common.Network;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;

namespace BerserkV3.Startup.Authorization
{
	public class AuthResetPassState : AuthState<AuthResetPassArgs>
	{
		private static AuthResetPassView Window => AuthResetPassView.Instance;

		protected override void OnEnter(AuthResetPassArgs args)
		{
			Window.SetCodeInputValueChangeAction(OnInputsChanged);
			Window.SetPassInputValueChangeAction(OnInputsChanged);
			Window.SetReturnAction(StateMachineBus.Switch<AuthForgotPassState>);
			Window.SetFooterAction(StateMachineBus.Switch<AuthSignInState>);
			Window.SetResetAction(() => ReqestResetPassAsync().Forget(e => RRLogger.Error(e)));
			Window.SetHeaderText("<b>Reset</b> Password");
			Window.SetMessageText($"We have sent you an email with reset code.");
			Window.SetFooterText($"Remember your Password? <color=#F55D0D>Log in");
			Window.SetResetText("Reset Password");
			Window.SetResetInteractable(false);
			Window.SetCodeInputText(string.Empty);
			Window.SetPassInputText(string.Empty);
			OnInputsChanged(string.Empty);
			Window.Show();
		}

		protected override void OnExit(AuthResetPassArgs args)
		{
			Window.Close();
		}

		private void OnInputsChanged(string value)
		{
			var isActive = !string.IsNullOrEmpty(Window.GetCodeInputText())
			               && !string.IsNullOrEmpty(Window.GetPassInputText());
			Window.SetResetInteractable(isActive);
		}

		private async UniTask ReqestResetPassAsync()
		{
			var email = GetArgs().Email;
			var model = new ResetPasswordModel
			{
				Email = email, 
				Password = Window.GetPassInputText(), 
				EmailCode = Window.GetCodeInputText()
			};
			
			var response = await IdentityAPI.PostResetPassword(model).AddLoadingTask();
			if (response.Code != HttpStatusCode.OK)
			{
				NotifyAndRetry(response.GetMessage());
				return;
			}

			var message = "Password reset successful! Continue to login.";
			var messageArgs = AuthMessageArgs.Accepted(nameof(AuthSignInState), message);
			var signInArgs = new AuthSignInArgs {Email = email, Password = Window.GetPassInputText()};
			StateMachineBus.Switch<AuthMessageState>(messageArgs, signInArgs);
		}

		private void NotifyAndRetry(string message)
		{
			var messageArgs = AuthMessageArgs.Retry(Id, message);
			StateMachineBus.Switch<AuthMessageState>(messageArgs, GetArgs());
		}
	}
}