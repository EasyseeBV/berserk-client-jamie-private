using Zenject;

namespace BerserkV3.Lobby.Network
{
	public class LobbyNetworkInstaller : Installer<LobbyNetworkInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<LobbySignalProcessor>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<LobbyHub>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<LobbyNetworkApplication>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<SignalTimeoutProcessor>()
				.AsTransient()
				.NonLazy();
		}
	}
}