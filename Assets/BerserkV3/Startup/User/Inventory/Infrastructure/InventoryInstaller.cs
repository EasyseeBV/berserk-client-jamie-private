using BerserkV3.Startup.Authorization.Inventory.Realizations;
using Zenject;

namespace BerserkV3.Startup.Authorization.Inventory.Infrastructure
{
	public class InventoryInstaller : Installer<InventoryInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<InventoryApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}