using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.Data.UserInventory
{
	public class OwnedConsumables : IInventoryItem
	{
		public string Id { get; set;}
		public int Amount { get; set;}
	}
}