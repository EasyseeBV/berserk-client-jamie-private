using Berserk.Shared.Data.Shop.Enums;

namespace Berserk.Shared.Data.Shop
{
	public class ItemModel
	{
		public string ItemId { get; set; }
		public string ArtUrl { get; set; }
		public ItemType Type { get; set; }
		public int Amount { get; set; }
		public int PriceWeight { get; set; } = 1;
	}
}