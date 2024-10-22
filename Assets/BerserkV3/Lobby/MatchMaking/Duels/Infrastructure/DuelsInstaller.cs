using BerserkV3.Lobby.MatchMaking.Sessions;
using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelsInstaller : Installer<DuelsInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<DuelsApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<DuelsSignalProcessor>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<DuelJoinApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<DuelCreateApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<DuelSelectDeckApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<DuelRoomApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}