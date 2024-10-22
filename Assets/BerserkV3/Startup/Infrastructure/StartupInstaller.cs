using BerserkV3.Common.TutorialSystem;
using BerserkV3.Lobby.Decks;
using BerserkV3.Lobby.MatchMaking.Practice;
using BerserkV3.Lobby.MatchMaking.Sessions;
using BerserkV3.Lobby.Vulcanite.Infrastructure;
using BerserkV3.Startup.Applications;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.Authorization.ExternalProviders;
using Zenject;

namespace BerserkV3.Startup.Infrastructure
{
	public class StartupInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<SetupStartupUIGroup>()
				.AsSingle()
				.NonLazy();
			
			DeckInstaller.Install(Container);
			VulcaniteInstaller.Install(Container);
			SessionsStartupInstaller.Install(Container);
			PracticeInstaller.Install(Container);
			
			Container
				.BindInterfacesTo<StartupApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<StartupRedirectionApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<GuestApplication>()
				.AsSingle()
				.NonLazy();			
			
			Container
				.BindInterfacesTo<FirstVulcaniteApplication>()
				.AsSingle()
				.NonLazy();
						
			Container
				.BindInterfacesTo<MessageApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<AuthenticationApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<ExternalProviderFactory>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<TutorialStartupHandlersInstaller>()
				.AsSingle()
				.NonLazy();
		}
	}
}