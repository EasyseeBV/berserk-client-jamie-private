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
	public class AuthReferralState : AuthState<AuthReferralArgs>
	{
		private readonly IUIService uiService;
		public AuthReferralState(IUIService uiService)
		{
			this.uiService = uiService;
		}

		protected override void OnEnter(AuthReferralArgs args)
		{
			uiService.Begin<AuthReferralWindow>()
				.WithInit(InitWindow)
				.Show();
			
			return;
			void InitWindow(AuthReferralWindow window)
			{
				window.SetHeaderText("Referral Link");
				window.SetMessageText("If you have referral link add here or skip");
				window.SetSkipText(args.Skip.Text);
				window.SetSubmitText(args.Submit.Text);
				window.SetSkipAction(() => HandleButton(args.Skip));
				window.SetSubmitAction(() => ValidateReferralAsync().Forget(e => RRLogger.Error(e)));
				window.SetInputChangeAction(v => window.SetSubmitInteractable(!string.IsNullOrEmpty(v)));
				window.SetInputText(string.Empty);
				window.SetSubmitInteractable(false);
			}
		}

		protected override void OnExit(AuthReferralArgs args)
		{
			uiService.Begin<AuthReferralWindow>().Hide();
		}

		private void NotifyAndRetry(string message)
		{
			AddStateArgs(AuthMessageArgs.Retry(Id, message));
			StateMachineBus.Switch<AuthMessageState>(GetRawArgs());
		}
		
		private async UniTask ValidateReferralAsync()
		{
			var stateAgrs = GetArgs();
			var verifyReferralModel = new VerifyReferralModel
			{
				Email = stateAgrs.Email,
				UserName = stateAgrs.UserName
			};
			
			var response = await IdentityAPI.PostVerifyReferral(verifyReferralModel).AddLoadingTask();
			if (response.Code != HttpStatusCode.OK)
			{
				NotifyAndRetry(response.GetMessage());
				return;
			}
			
			HandleButton(stateAgrs.Submit);
		}
	}
}