using Zenject;

namespace BerserkV3.GameCore.Customisations
{
	public class GameCustomisationInstaller : Installer<GameCustomisationInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<GameCustomisationApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.Bind<GameCustomisationsAdapter>()
				.AsSingle()
				.NonLazy();
		}
	}
}