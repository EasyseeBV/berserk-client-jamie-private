using BerserkV3.Common.LiveLinkRouter;
using Zenject;

namespace BerserkV3.Generic
{
	public class LiveLinkRouterInstaller : Installer<LiveLinkRouterInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<LiveLinkRouterService>()
				.AsSingle();
			
			Container
				.Bind<LiveLinkRouterAdapter>()
				.AsSingle()
				.NonLazy();
		}
	}
}