using System;
using System.Collections.Generic;
using System.Linq;

namespace Berserk.Shared.Data.Shop
{
	public class ShopTabLightModel
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Title { get; set; } = "EmptyName";
		public string Description { get; set; } = "";
		public string ParentShopTabId { get; set; }
		public bool IsEditing { get; set; }
		public int OrderAsc { get; set; }
		public bool IsMock { get; set; }
		public bool IsActive { get; set; }
		public bool IsComingSoon { get; set; }
		public bool IsFeatured { get; set; }
		
		public List<ShopTabLightModel> SubTabs { get; set; } = new();
		public bool IsCategory => SubTabs.Any();
	}
}