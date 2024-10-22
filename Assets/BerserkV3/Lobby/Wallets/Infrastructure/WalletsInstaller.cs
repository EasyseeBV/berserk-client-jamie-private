using BerserkV3.Lobby.Wallets.Realizations;
using Zenject;

namespace BerserkV3.Lobby.Wallets.Infrastructure
{
	public class WalletsInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<WalletsApplication>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<WalletsRepository>().AsSingle();
		}
	}
}