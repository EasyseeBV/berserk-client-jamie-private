using System;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.Utils;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.Decks;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Authorization.Inventory.Models;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using BerserkV3.Startup.Utils;
using Cysharp.Threading.Tasks;
using RR.UIService;

namespace BerserkV3.Startup.Authorization
{
	public class AuthProcessingState : AuthState<AuthProcessingArgs>
	{
		private readonly IUIService uiService;
		private readonly ISerializeHelper serializeHelper;
		private readonly IDeckApplication deckApplication;
		private readonly IMessageApplication messageApplication;
		private readonly IAnalyticsApplication analyticsApplication;
		private readonly ICustomisationApplication customisationApplication;
		private readonly IInventoryApplication inventoryApplication;

		public AuthProcessingState(
			IUIService uiService,
			ISerializeHelper serializeHelper,
			IDeckApplication deckApplication,
			IMessageApplication messageApplication,
			IAnalyticsApplication analyticsApplication,
			ICustomisationApplication customisationApplication,
			IInventoryApplication inventoryApplication)
		{
			this.uiService = uiService;
			this.serializeHelper = serializeHelper;
			this.deckApplication = deckApplication;
			this.messageApplication = messageApplication;
			this.analyticsApplication = analyticsApplication;
			this.customisationApplication = customisationApplication;
			this.inventoryApplication = inventoryApplication;
		}

		protected override void OnEnter(AuthProcessingArgs args)
		{
			ProcessDataAsync(args).Forget(DefaultSharedLogger.Error);
		}

		protected override void OnExit(AuthProcessingArgs args)
		{
			uiService.Begin<AuthSaveWindow>().Hide();
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
				await TaskUtil.RetryAsync(FetchInventory);
				await deckApplication.InitActiveDeckAsync(); 
				await TaskUtil.RetryAsync(FetchCustomisations).AddLoadingTask();// TODO remove
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
			return TryResult.Success;
		}

		private async Task<TryResult> FetchCustomisations()
		{
			return await customisationApplication.InitAsync()
				? TryResult.Success
				: TryResult.Retry;
		}
		
		private async Task<TryResult> FetchInventory()
		{
			var result = await inventoryApplication.InitAsync()
				? TryResult.Success
				: TryResult.Retry;
			
			return result;
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
			uiService.Begin<AuthSaveWindow>()
				.WithInit(InitWindow)
				.Show();
			
			if (await tcs.Task)
				AuthSave();
			
			await uiService.Begin<AuthSaveWindow>().HideAsync();
			
			return;
			void InitWindow(AuthSaveWindow window)
			{
				window.SetHeaderText("Authorization");
				window.SetMessageText(GameDatabase.GetLocalization("AuthRemember"));
				window.SetAcceptButtonText("Remember me");
				window.SetCancelButtonText("Skip");
				window.TimerWidget.SetTimer(30, () => tcs.TrySetResult(false));
				window.SetAcceptAction(() => tcs.TrySetResult(true));
				window.SetCancelAction(() => tcs.TrySetResult(false));
			}
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