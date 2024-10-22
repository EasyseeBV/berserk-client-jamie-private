using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;

namespace BerserkV3.Startup.Authorization.Inventory.Models
{
	public interface IInventoryItemRepository
	{
		public List<IInventoryItem> Items { get; }
	}
}