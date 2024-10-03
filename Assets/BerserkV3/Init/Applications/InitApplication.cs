using System;
using System.Net;
using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Abstractions;
using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Common.AppTime;
using BerserkV3.Common.Network;
using BerserkV3.Common.ProgressDrawer;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Network;
using BerserkV3.Generic.Customisation;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.UI;
using BerserkV3.Startup.Utils;
using BestHTTP;
using Cysharp.Threading.Tasks;
using Events;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using RR.Core.Utilities.DiscSpaceUtils;
using UnityEngine;
using Zenject;
using Environment = BerserkV3.Startup.Network.Enums.Environment;

namespace BerserkV3.Init.Applications
{
	public class InitApplication : DisposableWithCts, IInitializable
	{
		private readonly IGameDatabase gameDatabase;
		private readonly ISharedConfig sharedConfig;
		private readonly IProgressDrawer progressDrawer;
		private readonly IResourceService resourceService;
		private readonly IMessageApplication messageApplication;
		private readonly IAppTimeSynchronizer appTimeSynchronizer;
		private readonly IRedirectionApplication redirectionApplication;
		private readonly IAnalyticsApplication analyticsApplication;

		protected InitApplication(
			IGameDatabase gameDatabase,
			ISharedConfig sharedConfig,
			IProgressDrawer progressDrawer,
			IResourceService resourceService,
			IMessageApplication messageApplication,
			IAppTimeSynchronizer appTimeSynchronizer,
			IRedirectionApplication redirectionApplication,
			IAnalyticsApplication analyticsApplication)
		{
			this.gameDatabase = gameDatabase;
			this.sharedConfig = sharedConfig;
			this.progressDrawer = progressDrawer;
			this.resourceService = resourceService;
			this.messageApplication = messageApplication;
			this.redirectionApplication = redirectionApplication;
			this.appTimeSynchronizer = appTimeSynchronizer;
			this.analyticsApplication = analyticsApplication;
		}

		public async void Initialize()
		{
			try
			{
				HTTPManager.Setup();
				if (EnvironmentSwitcher.CurrentEnvironment <= Environment.Staging)
					await ServerChoiceView.Instance.Init();

				if (EnvironmentSwitcher.CurrentEnvironment > Environment.LocalHost)
					HTTPManager.RequestTimeout = TimeSpan.FromSeconds(60);

				await analyticsApplication.InitAsync(Token).AddLoadingTask();
				await TaskUtil.RetryLoopAsync(() => ServerRouter.SelectLowestLatencyServer(messageApplication));
				await TaskUtil.RetryLoopAsync(CheckVersion);
				await TaskUtil.RetryLoopAsync(appTimeSynchronizer.SynchronizeTimeAsync).AddLoadingTask();
				await TaskUtil.RetryAsync(FetchGameConfigs).AddLoadingTask();
				await TaskUtil.RetryAsync(FetchGameDataBase).AddLoadingTask();
				await TaskUtil.RetryLoopAsync(CheckUpdates);
				await redirectionApplication.RedirectAsync();
			}
			catch (OperationCanceledException e)
			{
				Initialize();
				DefaultSharedLogger.Error(e);
			}
			catch (Exception e)
			{
				await messageApplication.Critial();
				DefaultSharedLogger.Error(e);
			}
		}

		private async Task<TryResult> FetchGameDataBase()
		{
			if (gameDatabase.Initialized)
				return TryResult.Success;

			try
			{
				var response = await GameAPI.GetGameDataBase();

				if (!response.IsSuccess)
					return TryResult.Retry;

				gameDatabase.FillFrom(response.Data);
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return TryResult.Error;
			}

			return TryResult.Success;
		}
		
		private async Task<TryResult> FetchGameConfigs()
		{
			if (sharedConfig.Initialized)
				return TryResult.Success;

			try
			{
				var response = await GameAPI.GetGameConfig();

				if (!response.IsSuccess)
					return TryResult.Retry;

				sharedConfig.FillFromJson(response.Data);
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return TryResult.Error;
			}

			return TryResult.Success;
		}

		private async Task<TryResult> CheckVersion()
		{
			
			try
			{
				if (EnvironmentSwitcher.CurrentEnvironment <= Environment.Staging)
					return TryResult.Success;
				
				var response = await IdentityAPI.PostVerifyVersion().AddLoadingTask();

				switch (response.Code)
				{
					case HttpStatusCode.OK:
						return TryResult.Success;

					case 0:
					case HttpStatusCode.RequestTimeout:
					case HttpStatusCode.GatewayTimeout:
						return TryResult.Retry;

					default:
						await messageApplication.Critial(response.GetMessage());
						return TryResult.Cancel;
				}
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return TryResult.Error;
			}
		}

		private IProgress<float> SetupProgressScreen()
		{
			progressDrawer.Reset();
			var contentProgress = new ContentUpdateProgress();
			contentProgress.OnStart += () => progressDrawer.SetTooltip("Updating may take additional time..");
			progressDrawer.AddProgress(contentProgress);
			progressDrawer.ShowAsync().Forget();
			return contentProgress;
		}

		private async Task<TryResult> CheckUpdates()
		{
			try
			{
				var progress = SetupProgressScreen();
				await resourceService.InitializeAsync(progress);
				CustomisationBus.OnMusicUpdated += DataBus.AppData.Value.LobbyMusic;
			}
			catch (NotEnoughMemoryException e)
			{
				DefaultSharedLogger.Error(e);
				await messageApplication.Info(e.Message);
				return TryResult.Retry;
			}
			catch (InternetConnectionException e)
			{
				DefaultSharedLogger.Error(e);
				await messageApplication.Info(e.Message);
				return TryResult.Retry;
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return TryResult.Error;
			}
			finally
			{
				await progressDrawer.CloseAsync(true);
				progressDrawer.Reset();
			}

			DefaultSharedLogger.Log($"[{resourceService.GetType().Name.Orange()}] {"Initialized".Green()}.");
			return TryResult.Success;
		}
	}
}