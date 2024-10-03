using BerserkV3.Common.StateMachine;
using BerserkV3.Startup.UI;

namespace BerserkV3.Startup.Authorization
{
	public class AuthVerifyPassState : State
	{
		private static AuthVerifyPassView Window => AuthVerifyPassView.Instance;
		public override void OnEnter(params object[] args)
		{
			Window.SetHeaderText("<b>Reset</b> Password");
			Window.SetMessageText("We have sent you an email with reset password link,<br>" +
			                      "please click on that link to proceed further.");
			Window.SetFooterText($"Remember your Password? <color=#F55D0D>Log in");
			Window.SetFooterAction(StateMachineBus.Switch<AuthSignInState>);
			Window.SetReturnAction(StateMachineBus.Switch<AuthForgotPassState>);
			Window.SetFooterInteractable(true);
			Window.Show();
		}

		public override void OnExit()
		{
			Window.Close();
		}
	}
}