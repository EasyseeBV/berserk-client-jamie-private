using BerserkV3.Common.TutorialSystem;
using BerserkV3.Lobby.Decks;
using BerserkV3.Lobby.MatchMaking.AutoMatching;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Lobby.MatchMaking.Practice;
using BerserkV3.Lobby.MatchMaking.Sessions;
using BerserkV3.Lobby.Network;
using BerserkV3.Lobby.UI.General.Profile;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Controllers;
using BerserkV3.Lobby.Vulcanite.Infrastructure;
using BerserkV3.Startup.Applications;
using Statistics;
using Zenject;

namespace BerserkV3.Lobby.Home.Infrastructure
{
	public class LobbyHomeInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<SetupLobbyUIGroup>()
				.AsSingle()
				.NonLazy();
			
			DeckInstaller.Install(Container);
			SessionsLobbyInstaller.Install(Container);
			DuelsInstaller.Install(Container);
			PracticeInstaller.Install(Container);
			AutoMatchInstaller.Install(Container);
			LobbyNetworkInstaller.Install(Container);
			VulcaniteInstaller.Install(Container);
			
			Container
				.BindInterfacesTo<StatisticRepository>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<StatisticApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<LobbyRedirectionApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<LobbyApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<FirstVulcaniteApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<GuestApplication>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<TutorialLobbyHandlersInstaller>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<LobbySettingsApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<CurrencyPanelPresenter>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<MainMenuApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<PurchaseWindowApplication>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<PageSwitcher>()
				.AsTransient();
		}
	}
}