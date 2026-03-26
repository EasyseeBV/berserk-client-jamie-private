using System;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Common.Network;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.Deck;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using BerserkV3.Startup.Utils;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Startup.Authorization
{
	public class AuthProcessingState : AuthState<AuthProcessingArgs>
	{
		private readonly ISerializeHelper serializeHelper;
		private readonly IDeckApplication deckApplication;
		private readonly IMessageApplication messageApplication;
		private readonly IAnalyticsApplication analyticsApplication;
		private readonly ICustomisationApplication customisationApplication;
		private static AuthSaveView Window => AuthSaveView.Instance;

		public AuthProcessingState(
			ISerializeHelper serializeHelper,
			IDeckApplication deckApplication,
			IMessageApplication messageApplication,
			IAnalyticsApplication analyticsApplication,
			ICustomisationApplication customisationApplication)
		{
			this.serializeHelper = serializeHelper;
			this.deckApplication = deckApplication;
			this.messageApplication = messageApplication;
			this.analyticsApplication = analyticsApplication;
			this.customisationApplication = customisationApplication;
		}

		protected override void OnEnter(AuthProcessingArgs args)
		{
			ProcessDataAsync(args).Forget(DefaultSharedLogger.Error);
		}

		protected override void OnExit(AuthProcessingArgs args)
		{
			Window.Close();
			base.OnExit(args);
		}

		private async UniTask ProcessDataAsync(AuthProcessingArgs arg)
		{
			var model = arg.AuthModel;
			User.Data.AccessToken = model.AccessToken;
			User.Data.RefreshToken = model.RefreshToken;

			try
			{
				if (!model.IsAcceptedPrivacyPolicy)
				{
					var authThermsArg = AuthThermsArgs.Default();
					authThermsArg.Accept.CallBack = () =>
					{
						model.IsAcceptedPrivacyPolicy = true;
						StateMachineBus.Switch<AuthProcessingState>(arg);
					};
					StateMachineBus.Switch<AuthThermsState>(authThermsArg);
					return;
				}
				
				await TaskUtil.RetryAsync(FetchUserDataAsync);
				await TaskUtil.RetryAsync(FetchCustomisations).AddLoadingTask();
				await AuthSaveAsync();
				StateMachineBus.Switch<AuthCompleteState>();
			}
			catch (Exception)
			{
				await messageApplication.Critial();
				throw;
			}
		}

		private async Task<TryResult> FetchUserDataAsync()
		{
			if (string.IsNullOrEmpty(User.AccessToken))
				return TryResult.Retry;

			var argModel = GetArgs().AuthModel;
			var response = await IdentityAPI.GetUserData().AddLoadingTask();
			if (!response.IsSuccess || response.Data == null)
				return TryResult.Retry;

			// TODO: IMPORTANT do not remove it. It will break guest or user flow.
			// fetched UserData had LastLogin dateTime
			// we check was User logged in or registered, it will different if was registered or logged in again.
			if (response.Data.LastLogin != argModel.PreviousLogin)
				response.Data.LastLogin = argModel.PreviousLogin;

			User.Sync(response.Data);
			analyticsApplication.SetUserId(User.Id);
			AnalyticsBus.SendCustomEvent.Publish(new LoginModel(User.UserName));
			await deckApplication.InitActiveDeckAsync();
			return TryResult.Success;
		}

		private async Task<TryResult> FetchCustomisations()
		{
			return await customisationApplication.InitAsync()
				? TryResult.Success
				: TryResult.Retry;
		}

		private async UniTask AuthSaveAsync()
		{
			if (User.IsAnonymous)
				return;

			if (serializeHelper.HasKey(SerializeKeyHelper.REMEMBER_CREEDS))
			{
				AuthSave();
				return;
			}

			var tcs = new UniTaskCompletionSource<bool>();
			Window.Show();
			Window.SetHeaderText("Authorization");
			Window.SetMessageText(GameDatabase.GetLocalization("AuthRemember"));
			Window.SetAcceptButtonText("Remember me");
			Window.SetCancelButtonText("Skip");
			Window.TimerWidget.SetTimer(30, () => tcs.TrySetResult(false));
			Window.SetAcceptAction(() => tcs.TrySetResult(true));
			Window.SetCancelAction(() => tcs.TrySetResult(false));

			if (await tcs.Task)
				AuthSave();

			var closeAsync = new UniTaskCompletionSource();
			Window.Close(onAnimationDone: () => closeAsync.TrySetResult());
			await closeAsync.Task;
		}

		private void AuthSave()
		{
			serializeHelper.Patch(SerializeKeyHelper.REMEMBER_CREEDS, "true");
			serializeHelper.Patch(SerializeKeyHelper.ACCESS_TOKEN, User.AccessToken);
			serializeHelper.Patch(SerializeKeyHelper.REFRESH_TOKEN, User.RefreshToken);
			serializeHelper.Save();
		}
	}
}
