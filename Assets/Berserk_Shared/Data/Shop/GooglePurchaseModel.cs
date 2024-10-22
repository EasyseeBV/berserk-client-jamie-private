using System.Collections.Generic;

namespace Berserk.Shared.Data.Shop
{
	public class GooglePurchaseModel
	{
		public string PurchaseToken { get; set; }
		public string GoogleIApId { get; set; }
		public List<ItemModel> RewardList { get; set; }
	}
}