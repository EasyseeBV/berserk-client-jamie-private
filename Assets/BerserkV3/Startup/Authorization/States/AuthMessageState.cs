using BerserkV3.Startup.UI;

namespace BerserkV3.Startup.Authorization
{
	public class AuthMessageState : AuthState<AuthMessageArgs>
	{
		private static AuthMessageView Window => AuthMessageView.Instance;

		protected override void OnEnter(AuthMessageArgs args)
		{
			Window.SetSuccess(args.Success);
			Window.SetMessageText(args.Message);
			Window.SetHeaderText(args.Header);
			Window.SetButtonText(args.ButtonArg.Text);
			Window.SetOnSubmit(() => HandleButton(args.ButtonArg));
			Window.Show();
		}

		protected override void OnExit(AuthMessageArgs args)
		{
			Window.Close();
		}
	}
}