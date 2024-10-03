using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Common.AppTime;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.InputSystem;
using BerserkV3.Common.ProgressDrawer;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Common.StateMachine;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.UIKit.KeyboardHeightService;
using BerserkV3.Generic;
using BerserkV3.Generic.Customisation;
using BerserkV3.Generic.SharedLogger;
using BerserkV3.Generic.SystemDialogs;
using BerserkV3.Generic.UndoSystem;
using BerserkV3.Init.Applications;
using RR.Core.DebugSystem;
using Zenject;

namespace BerserkV3.Init.Infrastructure
{
	public class ProjectContextInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			SharedLoggerInstaller.Install(Container);
			
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
			AnalyticInstaller.Install(Container);
			LiveLinkRouterInstaller.Install(Container);
			TutorialInstaller.Install(Container);
			InputSystemInstaller.Install(Container);
			KeyboardHeightServiceInstaller.Install(Container);
		}
	}
}