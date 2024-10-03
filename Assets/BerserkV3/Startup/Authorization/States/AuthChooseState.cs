using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Network;
using BerserkV3.Common.StateMachine;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Startup.Authorization
{
	public class AuthChooseState : State
	{
		private static AuthChooseView Window => AuthChooseView.Instance;
		
		public override void OnEnter(params object[] args)
		{
			Window.Show();
			Window.SetSignInButtonText("LOGIN");
			Window.SetGuestButtonText("CONTINUE AS GUEST");
			Window.SetSignUpButtonText("SIGN-UP FOR FREE");
			Window.SetGuestAction(() => AuthSignInState.LoginGuest(response => NotifyAndRetry(response.GetMessage())).Forget(DefaultSharedLogger.Error));
			Window.SetSignInAction(StateMachineBus.Switch<AuthByTokenState>);
			Window.SetSignUpAction(StateMachineBus.Switch<AuthSignUpState>);
		}

		public override void OnExit()
		{
			Window.Close();
			base.OnExit();
		}
		
		private void NotifyAndRetry(string message)
		{
			StateMachineBus.Switch<AuthMessageState>(AuthMessageArgs.Retry(Id, message));
		}
	}
}