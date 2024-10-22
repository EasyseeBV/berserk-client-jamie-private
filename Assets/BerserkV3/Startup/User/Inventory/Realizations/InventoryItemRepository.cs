using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Startup.Authorization.Inventory.Models;

namespace BerserkV3.Startup.Authorization.Inventory.Realizations
{
	public class InventoryItemRepository : IInventoryItemRepository
	{
		public List<IInventoryItem> Items { get; } = new();
	}
}