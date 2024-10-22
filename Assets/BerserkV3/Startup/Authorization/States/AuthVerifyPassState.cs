using BerserkV3.Common.StateMachine;
using BerserkV3.Startup.UI;
using RR.UIService;

namespace BerserkV3.Startup.Authorization
{
	public class AuthVerifyPassState : State
	{
		private readonly IUIService uiService;

		public AuthVerifyPassState(IUIService uiService)
		{
			this.uiService = uiService;
		}

		public override void OnEnter(params object[] args)
		{
			uiService.Begin<AuthVerifyPassWindow>()
				.WithInit(InitWindow)
				.Show();

			return;

			void InitWindow(AuthVerifyPassWindow window)
			{
				window.SetHeaderText("<b>Reset</b> Password");
				window.SetMessageText("We have sent you an email with reset password link,<br>" +
				                      "please click on that link to proceed further.");
				window.SetFooterText($"Remember your Password? <color=#F55D0D>Log in");
				window.SetFooterAction(StateMachineBus.Switch<AuthSignInState>);
				window.SetReturnAction(StateMachineBus.Switch<AuthForgotPassState>);
				window.SetFooterInteractable(true);
			}
		}

		public override void OnExit()
		{
			uiService.Begin<AuthVerifyPassWindow>().Hide();
		}
	}
}