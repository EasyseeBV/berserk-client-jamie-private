using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Leagues
{
	public class LeagueInstaller : Installer<LeagueInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<LeagueApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.Bind<LeagueApplicationAdapter>() // TODO remove later
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<LeagueSignalProcessor>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<LeagueMatchSearchApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}