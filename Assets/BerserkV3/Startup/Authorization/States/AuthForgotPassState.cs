using System.Net;
using Berserk.Shared.Data.Identity;
using BerserkV3.Common.Network;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.UIService;

namespace BerserkV3.Startup.Authorization
{
	public class AuthForgotPassState : State
	{
		private readonly IUIService uiService;
		public AuthForgotPassState(IUIService uiService)
		{
			this.uiService = uiService;
		}

		public override void OnEnter(params object[] args)
		{
			uiService.Begin<AuthForgotPassWindow>()
				.WithInit(InitWindowAsync)
				.Show();
			
			return;
			void InitWindowAsync(AuthForgotPassWindow window)
			{
				window.SetInputValueChangeAction(v => window.SetResetInteractable(!string.IsNullOrEmpty(v)));
				window.SetReturnAction(StateMachineBus.Switch<AuthSignInState>);
				window.SetFooterAction(StateMachineBus.Switch<AuthSignInState>);
				window.SetResetAction(() => ReqestResetPassAsync().Forget(e => RRLogger.Error(e)));
				window.SetHeaderText("<b>Forgot</b> Password");
				window.SetFieldText("Email Address");
				window.SetFooterText($"Remember your Password? <color=#F55D0D>Log in");
				window.SetResetText("Reset Password");
				window.SetResetInteractable(false);
				window.SetInputText(string.Empty);
			}
		}

		public override void OnExit()
		{
			uiService.Begin<AuthForgotPassWindow>().Hide();
		}

		private async UniTask ReqestResetPassAsync()
		{
			var email = uiService.Get<AuthForgotPassWindow>().GetInputText();
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