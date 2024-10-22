using System.Collections.Generic;
using System.Linq;

namespace Berserk.Shared.Data.UserInventory
{
	public class UserInventoryModel
	{
		public List<OwnedCard> OwnedCards { get; set; } = new();
		public List<OwnedDeck> Decks { get; set; } = new();
		public List<OwnedVulcanite> OwnedVulcanites { get; set; } = new();
		public List<OwnedCustomization> OwnedCustomizations { get; set; } = new();
		public List<OwnedConsumables> OwnedConsumables { get; set; } = new();
	}
}