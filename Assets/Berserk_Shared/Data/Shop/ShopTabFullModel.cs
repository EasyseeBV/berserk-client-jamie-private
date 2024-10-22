using System.Collections.Generic;
using System.Linq;

namespace Berserk.Shared.Data.Shop
{
	public class ShopTabFullModel
	{
		public string Id { get; set; }
		public string MainProductId { get; set; }
		public ProductModel MainProduct { get; set; }
		public string ParentShopTabId { get; set; }
		
		public List<ShopTabFullModel> ChildShopTabs { get; set; } = new();
		
		public string Title { get; set; }
		public string Description { get; set; }
		public int OrderAsc { get; set; }
		public bool IsActive { get; set; }
		public bool IsComingSoon { get; set; }
		public bool IsFeatured { get; set; }

		public List<ProductModel> Products { get; set; } = new();

		public bool IsCategory => ChildShopTabs.Any();
	}
}