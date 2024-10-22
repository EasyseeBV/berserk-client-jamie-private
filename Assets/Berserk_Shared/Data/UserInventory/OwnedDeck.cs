using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.UserInventory
{
	public class OwnedDeck : IInventoryItem
	{
		public string Id { get; set; }
		public string OwnedVulcaniteId { get; set; }
		public string Name { get; set; }
		public float DeckValue { get; set; }
		public Faction Faction { get; set; }
		public List<string> OwnedCardIds { get; set; } = new();

		public void Fill(OwnedDeck other)
		{
			if (other == null)
				return;

			Id = other.Id;
			OwnedVulcaniteId = other.OwnedVulcaniteId;
			Name = other.Name; 
			DeckValue = other.DeckValue;
			Faction = other.Faction;
			OwnedCardIds = other.OwnedCardIds;
		}
	}
}