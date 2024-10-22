using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.Data.UserInventory
{
	public class OwnedVulcanite : IInventoryItem
	{
		public string Id { get; set;  }
		public string VulcaniteId { get; set; }
		public bool IsOwned { get; set; }
		public bool IsRent { get; set; }
		public bool IsExpireRent { get; set; }
	}
}