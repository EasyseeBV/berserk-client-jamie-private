using BerserkV3.Startup.UI;
using RR.UIService;

namespace BerserkV3.Startup.Authorization
{
	public class AuthMessageState : AuthState<AuthMessageArgs>
	{
		private readonly IUIService uiService;
		public AuthMessageState(IUIService uiService)
		{
			this.uiService = uiService;
		}

		protected override void OnEnter(AuthMessageArgs args)
		{
			uiService.Begin<AuthMessageWindow>()
				.WithInit(InitWindow)
				.Show();
			
			return;
			void InitWindow(AuthMessageWindow window)
			{
				window.SetSuccess(args.Success);
				window.SetMessageText(args.Message);
				window.SetHeaderText(args.Header);
				window.SetButtonText(args.ButtonArg.Text);
				window.SetOnSubmit(() => HandleButton(args.ButtonArg));
			}
		}

		protected override void OnExit(AuthMessageArgs args)
		{
			uiService.Begin<AuthMessageWindow>().Hide();
		}
	}
}