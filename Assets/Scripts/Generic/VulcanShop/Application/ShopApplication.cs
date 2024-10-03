using Lobby;
using System.Collections.Generic;
using System.Threading.Tasks;
using BerserkV3.Common.Utils;
using Vulcan.Shop.Domain;
using Vulcan.Shop.Service;

namespace Vulcan.Application
{
	public interface IShopApplication
	{
		bool RequiredRefresh { get; }

		double GetBalance();
		IEnumerable<VulcaniteEntity> GetShopVulcanites();
		bool PurchaseAvailable(VulcaniteEntity currentVulcanite);
		Task RefreshData();
		Task TryByuAsync(VulcaniteEntity currentVulcanite);
	}
	public class ShopApplication : IShopApplication
	{
		private IShopService shopService;

		public ShopApplication()
		{
			shopService = new ShopService();
		}

		public bool RequiredRefresh => shopService.RequiredRefresh;

		public async Task RefreshData()
		{
			if (!RequiredRefresh)
				return;

			await shopService.RefreshShopVulcanite();
			await shopService.RefreshPlayerBalance();
		}

		public double GetBalance() => shopService.LavaBalance.Value;

		public IEnumerable<VulcaniteEntity> GetShopVulcanites() => shopService.Vulcanites;

		public async Task TryByuAsync(VulcaniteEntity currentVulcanite)
		{
			if (currentVulcanite == null)
				return;

			await shopService.TryByuAsync(currentVulcanite).AddLoadingTask();
			// await VulcaniteHandler.FetchOwned();
		}

		public bool PurchaseAvailable(VulcaniteEntity currentVulcanite)
		{
			return !currentVulcanite.IsOwned && shopService.LavaBalance.Value >= currentVulcanite.Price;
		}
	}

	
}