using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Common.AppTime;
using BerserkV3.Common.AudioSystem.Infrastructure;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.EventSource.Infrastructure;
using BerserkV3.Common.InputSystem;
using BerserkV3.Common.ProgressDrawer;
using BerserkV3.Common.PurchasingSystem.Infrastructure;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.UIService.Infrastructure;
using BerserkV3.Common.WebView.Infrastructure;
using BerserkV3.Common.UIKit.KeyboardHeightService;
using BerserkV3.Common.UIKit.NotifyService.Infrastructure;
using BerserkV3.Generic;
using BerserkV3.Generic.Customisation;
using BerserkV3.Generic.SharedLogger;
using BerserkV3.Generic.SystemDialogs;
using BerserkV3.Generic.UndoSystem;
using BerserkV3.Init.Applications;
using BerserkV3.Startup.Authorization.Inventory.Infrastructure;
using Zenject;

namespace BerserkV3.Init.Infrastructure
{
	public class ProjectContextInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			SharedLoggerInstaller.Install(Container);
			InstallUserReporting(); // To be able to use inventory application thru DI in reporting logic
			
			Container
				.BindInterfacesTo<SetupApplication>()
				.AsSingle()
				.NonLazy();
			
			AppTimeInstaller.Install(Container);
			StateMachineInstaller.Install(Container);
			SceneServiceInstaller.Install(Container);
			SerializeHelperInstaller.Install(Container);
			SystemDialogsInstaller.Install(Container);
			ResourceServiceInstaller.Install(Container);
			CustomisationInstaller.Install(Container);
			UndoSystemInstaller.Install(Container);
			ProgressDrawerInstaller.Install(Container);
			DataBaseInstaller.Install(Container);
			EventSourceInstaller.Install(Container);
			AnalyticInstaller.Install(Container);
			PurchasingInstaller.Install(Container);
			LiveLinkRouterInstaller.Install(Container);
			TutorialInstaller.Install(Container);
			InputSystemInstaller.Install(Container);
			UIServiceInstaller.Install(Container);
			NotifyServiceInstaller.Install(Container);
			AudioSystemInstaller.Install(Container);
			InventoryInstaller.Install(Container);
			WebViewApplicationInstaller.Install(Container);
			KeyboardHeightServiceInstaller.Install(Container);
		}

		private void InstallUserReporting()
		{
			var userReportingScriptObject = FindObjectOfType<UserReportingScript>();

			if (userReportingScriptObject != null)
			{
				Container.Bind<UserReportingScript>().FromInstance(userReportingScriptObject).AsSingle();
			}
		}
	}
}