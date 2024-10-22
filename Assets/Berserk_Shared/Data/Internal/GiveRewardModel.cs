using System.Collections.Generic;
using Berserk.Shared.Data.Shop;

namespace Berserk.Shared.Data.Internal
{
	public class GiveRewardModel
	{
		public string UserId { get; set; }
		public List<ItemModel> RewardList { get; set; }
	}
}