using System.Net;
using Berserk.Shared.Data.Identity;
using BerserkV3.Common.Network;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;

namespace BerserkV3.Startup.Authorization
{
	public class AuthReferralState : AuthState<AuthReferralArgs>
	{
		private static AuthReferralView Window => AuthReferralView.Instance;

		protected override void OnEnter(AuthReferralArgs args)
		{
			Window.SetHeaderText("Referral Link");
			Window.SetMessageText("If you have referral link add here or skip");
			Window.SetSkipText(args.Skip.Text);
			Window.SetSubmitText(args.Submit.Text);
			Window.SetSkipAction(() => HandleButton(args.Skip));
			Window.SetSubmitAction(() => ValidateReferralAsync().Forget(e => RRLogger.Error(e)));
			Window.SetInputChangeAction(v => Window.SetSubmitInteractable(!string.IsNullOrEmpty(v)));
			Window.SetInputText(string.Empty);
			Window.SetSubmitInteractable(false);
			Window.Show();
		}

		protected override void OnExit(AuthReferralArgs args)
		{
			Window.Close();
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