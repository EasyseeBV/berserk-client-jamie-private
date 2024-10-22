using BerserkV3.Lobby.Vulcanite.Realization;
using Zenject;

namespace BerserkV3.Lobby.Vulcanite.Infrastructure
{
	public class VulcaniteInstaller : Installer<VulcaniteInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<VulcaniteApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}