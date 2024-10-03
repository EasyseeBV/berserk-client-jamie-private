using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Sessions
{
	public class SessionsLobbyInstaller : Installer<SessionsLobbyInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<SessionsApplication>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<SessionsSignalProcessor>()
				.AsSingle()
				.NonLazy();
		}
	}
}