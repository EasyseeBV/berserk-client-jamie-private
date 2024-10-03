using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Sessions
{
	public class SessionsStartupInstaller : Installer<SessionsStartupInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<SessionsApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<SessionSignalMockProcessor>()
				.AsSingle()
				.NonLazy();
		}
	}
}