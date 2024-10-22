using BerserkV3.Lobby.Store.Abstractions;
using BerserkV3.Lobby.Store.Realizations;
using Zenject;

namespace BerserkV3.Lobby.Store.Infrastructure
{
	public class StoreInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<StoreApplication>()
				.AsSingle();

			Container.
				BindInterfacesTo<StoreRepository>()
				.AsSingle();
		}
	}
}