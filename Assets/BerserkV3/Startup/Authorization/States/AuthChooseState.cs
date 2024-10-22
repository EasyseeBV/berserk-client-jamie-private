using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Network;
using BerserkV3.Common.StateMachine;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.UIService;

namespace BerserkV3.Startup.Authorization
{
	public class AuthChooseState : State
	{
		private readonly IUIService uiService;
		public AuthChooseState(IUIService uiService)
		{
			this.uiService = uiService;
		}

		public override void OnEnter(params object[] args)
		{
			uiService.Begin<AuthChooseWindow>()
				.WithInit(InitWindow)
				.Show();
			
			return;
			void InitWindow(AuthChooseWindow window)
			{
				window.SetSignInButtonText("LOGIN");
				window.SetGuestButtonText("CONTINUE AS GUEST");
				window.SetSignUpButtonText("SIGN-UP FOR FREE");
				window.SetGuestAction(() => AuthSignInState.LoginGuest(response => NotifyAndRetry(response.GetMessage())).Forget(DefaultSharedLogger.Error));
				window.SetSignInAction(StateMachineBus.Switch<AuthByTokenState>);
				window.SetSignUpAction(StateMachineBus.Switch<AuthSignUpState>);
			}
		}

		public override void OnExit()
		{
			uiService.Begin<AuthChooseWindow>().Hide();
			base.OnExit();
		}
		
		private void NotifyAndRetry(string message)
		{
			StateMachineBus.Switch<AuthMessageState>(AuthMessageArgs.Retry(Id, message));
		}
	}
}