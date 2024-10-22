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
	public class AuthResetPassState : AuthState<AuthResetPassArgs>
	{
		private readonly IUIService uiService;
		public AuthResetPassState(IUIService uiService)
		{
			this.uiService = uiService;
		}

		protected override void OnEnter(AuthResetPassArgs args)
		{
			uiService.Begin<AuthResetPassWindow>()
				.WithInit(InitWindow)
				.Show();
			
			return;
			void InitWindow(AuthResetPassWindow window)
			{
				window.SetCodeInputValueChangeAction(InputsChanged);
				window.SetPassInputValueChangeAction(InputsChanged);
				window.SetReturnAction(StateMachineBus.Switch<AuthForgotPassState>);
				window.SetFooterAction(StateMachineBus.Switch<AuthSignInState>);
				window.SetResetAction(() => ReqestResetPassAsync().Forget(e => RRLogger.Error(e)));
				window.SetHeaderText("<b>Reset</b> Password");
				window.SetMessageText($"We have sent you an email with reset code.");
				window.SetFooterText($"Remember your Password? <color=#F55D0D>Log in");
				window.SetResetText("Reset Password");
				window.SetResetInteractable(false);
				window.SetCodeInputText(string.Empty);
				window.SetPassInputText(string.Empty);
				InputsChanged(string.Empty);
				
				return;
				void InputsChanged(string value)
				{
					var isActive = !string.IsNullOrEmpty(window.GetCodeInputText())
					               && !string.IsNullOrEmpty(window.GetPassInputText());
					window.SetResetInteractable(isActive);
				}
			}
		}

		protected override void OnExit(AuthResetPassArgs args)
		{
			uiService.Begin<AuthResetPassWindow>().Hide();
		}

		private async UniTask ReqestResetPassAsync()
		{
			var window = uiService.Get<AuthResetPassWindow>();
			var email = GetArgs().Email;
			var model = new ResetPasswordModel
			{
				Email = email, 
				Password = window.GetPassInputText(), 
				EmailCode = window.GetCodeInputText()
			};
			
			var response = await IdentityAPI.PostResetPassword(model).AddLoadingTask();
			if (response.Code != HttpStatusCode.OK)
			{
				NotifyAndRetry(response.GetMessage());
				return;
			}

			var message = "Password reset successful! Continue to login.";
			var messageArgs = AuthMessageArgs.Accepted(nameof(AuthSignInState), message);
			var signInArgs = new AuthSignInArgs {Email = email, Password = window.GetPassInputText()};
			StateMachineBus.Switch<AuthMessageState>(messageArgs, signInArgs);
		}

		private void NotifyAndRetry(string message)
		{
			var messageArgs = AuthMessageArgs.Retry(Id, message);
			StateMachineBus.Switch<AuthMessageState>(messageArgs, GetArgs());
		}
	}
}