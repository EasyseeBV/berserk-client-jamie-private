using BerserkV3.Common.PurchasingSystem.Models;

namespace BerserkV3.Lobby.UI.Store.Models
{
	public class StoreProductViewModel
	{
		public string Id { get; }
		public ProductPriceModel Price { get; }

		public StoreProductViewModel(string id, ProductPriceModel price)
		{
			Id = id;
			Price = price;
		}
	}
}