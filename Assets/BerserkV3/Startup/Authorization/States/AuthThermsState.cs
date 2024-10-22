using System;
using System.Net;
using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Common.LiveLinkRouter;
using BerserkV3.Common.Network;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using BestHTTP;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Network.Rest;
using RR.UIService;
using UnityEngine.Device;

namespace BerserkV3.Startup.Authorization
{
	public class AuthThermsState : AuthState<AuthThermsArgs>
	{
		private readonly IUIService uiService;
		public AuthThermsState(IUIService uiService)
		{
			this.uiService = uiService;
		}

		protected override async void OnEnter(AuthThermsArgs args)
		{
			var policyText = await GetThermsAsync().AddLoadingTask();
			uiService.Begin<AuthThermsWindow>()
				.WithInit(InitWindow)
				.Show();
			
			return;

			void InitWindow(AuthThermsWindow window)
			{
				if (!string.IsNullOrEmpty(policyText))
					window.SetMessageText(policyText);
			
				window.SetHeaderText("Please Read the Terms and Privacy Policy");
				window.SetButtonText(args.Accept.Text);
				window.SetToggleChangeAction(window.SetButtonInteractable);
				window.SetSubmitAction(() => AcceptThermsAsync().Forget(e => RRLogger.Error(e)));
				window.SetFooterAction(() => HandleButton(args.Footer));
				window.SetFooterText(args.Footer.Text);
				window.SetButtonInteractable(false);
				window.SetToggle(false);
			}
		}

		protected override void OnExit(AuthThermsArgs args)
		{
			uiService.Begin<AuthThermsWindow>().Hide();
		}
		
		private async UniTask AcceptThermsAsync()
		{
			var retry = 3;
			var response = new APIResponse<string> {IsSuccess = false};
			
			while (Application.isPlaying && !response && !Token.IsCancellationRequested && retry > 0)
			{
				try
				{
					response = await IdentityAPI.PostAcceptPrivacyPolicy().AddLoadingTask();
					if (response.Code != HttpStatusCode.OK)
						throw new Exception(response.GetMessage());
					
					User.Data.IsAcceptedPrivacyPolicy = true;
					AnalyticsBus.SendCustomEvent.Publish(new ThermsAcceptedModel(User.UserName));
					HandleButton(GetArgs().Accept);
					return;
				}
				catch (Exception e)
				{
					RRLogger.Error($"[{GetType().Name.Orange()}] {e}");
				}
				
				retry--;
				await UniTask.Delay(1000).AddLoadingTask(); // retry time
			}
			
			RRLogger.Error($"[{GetType().Name.Orange()}] {response.GetMessage()}");
			AddStateArgs(AuthMessageArgs.Retry(Id, response.GetMessage()));
			StateMachineBus.Switch<AuthMessageState>(GetRawArgs());
		}

		private async UniTask<string> GetThermsAsync()
		{
			var retry = 3;
			var uri = new Uri(GameDatabase.GetLink(LinkKeyHelper.THERMS).Link);
			while (Application.isPlaying && !Token.IsCancellationRequested && retry > 0)
			{

				try
				{
					var tcs = new UniTaskCompletionSource<string>();
					var responsehandler = new OnRequestFinishedDelegate((req, res) =>
					{
						if (res.IsSuccess)
						{
							tcs.TrySetResult(res.DataAsText);
							return;
						}

						tcs.TrySetException(req.Exception);
					});

					var request = new HTTPRequest(uri, responsehandler);
					request.Send();
					
					var result = await tcs.Task;
					if (string.IsNullOrEmpty(result))
						throw request.Exception;

					request.Dispose();
					return result;
				}
				catch (Exception e)
				{
					var retryText = $"retry : {retry--}".Red();
					RRLogger.Error($"[{GetType().Name.Orange()}] Can't load therms, {retryText}, Error : {e}");
				}
			}

			RRLogger.Error($"[{GetType().Name.Orange()}] Can't load therms.");
			return null;
		}
	}
}