using BerserkV3.Init.Applications;
using BerserkV3.Startup.Applications;
using Zenject;

namespace BerserkV3.Init.Infrastructure
{
	public class InitInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<SetupInitUIGroup>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<InitApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<InitRedirectionApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<MessageApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<ServerChoiseApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}