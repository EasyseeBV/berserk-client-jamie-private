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
	public class AuthForgotPassState : State
	{
		private static AuthForgotPassView Window => AuthForgotPassView.Instance;
		public override void OnEnter(params object[] args)
		{
			Window.SetInputValueChangeAction(v => Window.SetResetInteractable(!string.IsNullOrEmpty(v)));
			Window.SetReturnAction(StateMachineBus.Switch<AuthSignInState>);
			Window.SetFooterAction(StateMachineBus.Switch<AuthSignInState>);
			Window.SetResetAction(() => ReqestResetPassAsync().Forget(e => RRLogger.Error(e)));
			Window.SetHeaderText("<b>Forgot</b> Password");
			Window.SetFieldText("Email Address");
			Window.SetFooterText($"Remember your Password? <color=#F55D0D>Log in");
			Window.SetResetText("Reset Password");
			Window.SetResetInteractable(false);
			Window.SetInputText(string.Empty);
			Window.Show();
		}

		public override void OnExit()
		{
			Window.Close();
		}

		private async UniTask ReqestResetPassAsync()
		{
			var email = Window.GetInputText();
			var model = new ForgotPasswordModel { Email = email }; 
			var response = await IdentityAPI.PostForgotPassword(model).AddLoadingTask();
			if (response.Code != HttpStatusCode.OK)
			{
				NotifyAndRetry(response.GetMessage());
				return;
			}
			
			var resetPassArgs = new AuthResetPassArgs {Email = email};
			var message = "We have sent you an email with reset code.";
			var messageArgs = AuthMessageArgs.Accepted(nameof(AuthResetPassState), message);
			StateMachineBus.Switch<AuthMessageState>(messageArgs, resetPassArgs);
		}
		
		private void NotifyAndRetry(string message)
		{
			var messageArgs = AuthMessageArgs.Retry(Id, message);
			StateMachineBus.Switch<AuthMessageState>(messageArgs);
		}
	}
}