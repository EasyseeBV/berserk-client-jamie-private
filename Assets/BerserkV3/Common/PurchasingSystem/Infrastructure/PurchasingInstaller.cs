using BerserkV3.Common.PurchasingSystem.Realizations;
using Zenject;

namespace BerserkV3.Common.PurchasingSystem.Infrastructure
{
	public class PurchasingInstaller : Installer<PurchasingInstaller>
	{
		public override void InstallBindings()
		{
			InstallProviders();
			Container
				.BindInterfacesTo<PurchasingApplication>()
				.AsSingle()
				.Lazy();
		}

		private void InstallProviders()
		{
			Container
				.BindInterfacesTo<UnityPurchasingProvider>()
				.AsSingle();


			Container
				.BindInterfacesTo<StripePurchasingProvider>()
				.AsSingle();
		}
	}
}