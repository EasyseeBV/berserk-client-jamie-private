using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Lobby.Applications;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.LeaderBoard;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Lobby.MatchMaking.Leagues;
using BerserkV3.Lobby.MatchMaking.Practice;
using BerserkV3.Lobby.MatchMaking.Sessions;
using BerserkV3.Startup.Applications;
using Zenject;

namespace BerserkV3.Lobby.Infrastructure
{
	public class LobbyInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			DeckInstaller.Install(Container);
			LeaderBoardInstaller.Install(Container);
			SessionsLobbyInstaller.Install(Container);
			DuelsInstaller.Install(Container);
			PracticeInstaller.Install(Container);
			LeagueInstaller.Install(Container);
			PreviewSystemInstaller.Install(Container);
			LobbyNetworkInstaller.Install(Container);
			
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
		}
	}
}