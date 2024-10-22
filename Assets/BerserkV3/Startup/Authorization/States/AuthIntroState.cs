using BerserkV3.Common.StateMachine;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.UIService;

namespace BerserkV3.Startup.Authorization
{
	public class AuthIntroState : State
	{
		private readonly IUIService uiService;
		private const int INTRO_SHOW_DELAY_MS = 500;
		private const int INTRO_SHOWED_DELAY_MS = 1000;
		public AuthIntroState(IUIService uiService)
		{
			this.uiService = uiService;
		}

		public override async void OnEnter(params object[] args)
		{
			await UniTask.Delay(INTRO_SHOW_DELAY_MS);
			await uiService.Begin<AuthIntroWindow>().ShowAsync();
			await UniTask.Delay(INTRO_SHOWED_DELAY_MS);
			await uiService.Begin<AuthIntroWindow>().HideAsync();
			StateMachineBus.Switch<AuthChooseState>();
		}
	}
}