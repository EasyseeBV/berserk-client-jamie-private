using BerserkV3.Lobby.Applications;
using BerserkV3.Lobby.Network;
using Zenject;

namespace BerserkV3.Lobby.Infrastructure
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