using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Shop;

namespace BerserkV3.Lobby.Store.Utilities
{
	public class StoreUtils
	{
		public static float CalculateBundleDiscountPercentage(ICollection<string> userExistsItemIds,
			ICollection<ItemModel> rewardItems)
		{
			if (!rewardItems.Any() || rewardItems.All(x => x.PriceWeight == 0))
				return 0;

			var fullWeightSum = (float)rewardItems.Sum(x => x.PriceWeight);
			if (fullWeightSum == 0)
				return 0;

			var discountWeight = (float)rewardItems.Where(x => userExistsItemIds.Contains(x.ItemId))
				.Sum(x => x.PriceWeight);

			return (discountWeight / fullWeightSum) * 100;
		}

		public static float CalculateBundleDiscountPercentage(ProductModel productModel, ItemModel itemModel)
		{
			if (!productModel.RewardItems.Any() ||
			    !productModel.RewardItems.Contains(itemModel) ||
			    productModel.RewardItems.All(x => x.PriceWeight == 0))
			{
				return 0;
			}

			var fullWeightSum = (float)productModel.RewardItems.Sum(x => x.PriceWeight);
			if (fullWeightSum == 0)
			{
				return 0;
			}

			return (itemModel.PriceWeight / fullWeightSum) * 100;
		}
	}
}