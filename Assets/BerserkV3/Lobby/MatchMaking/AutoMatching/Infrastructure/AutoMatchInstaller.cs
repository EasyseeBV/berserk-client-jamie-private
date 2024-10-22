using Zenject;

namespace BerserkV3.Lobby.MatchMaking.AutoMatching
{
	public class AutoMatchInstaller : Installer<AutoMatchInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<AutoMatchApplication>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<AutoMatchSignalProcessor>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<AutoMatchSearchApplication>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<AutoMatchAcceptApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}