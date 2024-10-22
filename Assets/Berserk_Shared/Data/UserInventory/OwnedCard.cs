using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.Data.UserInventory
{
	public class OwnedCard : IInventoryItem
	{
		public string Id { get; set;}
		public string CardId { get; set;}
		public bool IsOwned { get; set; }
		public bool IsSubscription { get; set; }
		public bool IsNft { get; set; }
	}
}