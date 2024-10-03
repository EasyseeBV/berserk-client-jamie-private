using BerserkV3.GameCore.Customisations;
using Zenject;

namespace BerserkV3.Generic.Customisation
{
	public class CustomisationInstaller : Installer<CustomisationInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<CustomisationItemFactory>()
				.AsSingle();
			
			Container
				.BindInterfacesTo<CustomisationApplication>()
				.AsSingle();
			
			Container
				.BindInterfacesTo<CustomisationItemRepository>()
				.AsSingle();

			Container
				.Bind<CustomisationServiceAdapter>()
				.AsSingle()
				.NonLazy();
		}
	}
}