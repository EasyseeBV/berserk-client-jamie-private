using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.Data.UserInventory
{
	public class OwnedCustomization : IInventoryItem
	{
		public string Id { get; set; }
		public string CustomisationId { get; set; }
		public bool IsEquipped { get; set; }
	}
}