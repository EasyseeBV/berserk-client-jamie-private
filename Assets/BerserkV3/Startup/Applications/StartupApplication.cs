using System;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Abstractions;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Authorization;
using Zenject;

namespace BerserkV3.Startup.Applications
{
	public class StartupApplication : DisposableWithCts, IInitializable
	{
		private readonly IMessageApplication messageApplication;
		private readonly IRedirectionApplication redirectionApplication;

		protected StartupApplication(
			IMessageApplication messageApplication,
			IRedirectionApplication redirectionApplication)
		{
			this.messageApplication = messageApplication;
			this.redirectionApplication = redirectionApplication;
		}

		public async void Initialize()
		{
			try
			{
				if (await redirectionApplication.RedirectAsync())
					return;

				DefaultSharedLogger.Error("Redirection failed. Lets logout.");
				await User.LogOutAsync().AddLoadingTask();
				Initialize();
			}
			catch (OperationCanceledException e)
			{
				DefaultSharedLogger.Error(e);
			}
			catch (Exception e)
			{
				await messageApplication.Critial();
				DefaultSharedLogger.Error(e);
			}
		}
	}
}