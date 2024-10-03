using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.StateMachine;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Startup.Authorization
{
	public class AuthIntroState : State
	{
		private const int INTRO_DELAY_MS = 500;
		private const int INTRO_DURATION_MS = 1000;
		private static AuthIntroView Window => AuthIntroView.Instance;
		
		public override void OnEnter(params object[] args)
		{
			IntroAsync().Forget(DefaultSharedLogger.Error);
		}

		public override void OnExit()
		{
			Window.Close();
			base.OnExit();
		}

		private async UniTask IntroAsync()
		{
			await UniTask.Delay(INTRO_DELAY_MS);
			await Window.ShowAsync(1f);
			await UniTask.Delay(INTRO_DURATION_MS);
			await Window.CloseAsync(1f);
			StateMachineBus.Switch<AuthChooseState>();
		}
	}
}