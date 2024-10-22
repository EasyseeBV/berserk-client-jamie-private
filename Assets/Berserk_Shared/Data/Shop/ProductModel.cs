using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Shop.Enums;

namespace Berserk.Shared.Data.Shop
{
	public class ProductModel
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Title { get; set; }
		public string Description { get; set; }
		public string ArtUrl { get; set; }
		public string PreviewUrl { get; set; }
		public string BackgroundArtUrl { get; set; }
		public int OrderAsc { get; set; }
		public ProductType ProductType { get; set; }
		public ProductLayout ProductLayout { get; set; } = ProductLayout.Default;
		public ProductStatus Status { get; set; } = ProductStatus.Draft;
		public Size Size { get; set; } = Size.Small;
		public ColoredBackground ColoredBackground { get; set; } = ColoredBackground.None;
		public BorderType BorderType { get; set; } = BorderType.None;
		public bool IsOneTime { get; set; }
		public bool IsOutOfStock { get; set; }
		public bool IsBestValue { get; set; }
		public bool IsPopular { get; set; }
		public bool IsDetailedViewRequired { get; set; }

		#region FeatureOfferFields
		public DateTime? StartActiveDate { get; set; }
		public DateTime? EndActiveDate { get; set; }
		public bool IsOfferActive => (!StartActiveDate.HasValue || StartActiveDate < DateTime.UtcNow) &&
		                             (!EndActiveDate.HasValue || EndActiveDate > DateTime.UtcNow);
		#endregion

		#region BundleFields
		public bool HasDiscountPerOwnedItem { get; set; }
		#endregion

		#region SalesFields
		public DateTime? SaleStartDate { get; set; }
		public DateTime? SaleEndDate { get; set; }
		public bool IsOnSale => (SaleStartDate.HasValue && SaleStartDate < DateTime.UtcNow) &&
		                        (SaleEndDate.HasValue && SaleEndDate > DateTime.UtcNow);
		#endregion

		public List<ItemModel> RewardItems { get; set; } = new();

		public PriceWallet DefaultPrice => Prices[0];
		public List<PriceWallet> Prices { get; set; } = new();

		public bool IsActive => Status == ProductStatus.Publish && (ProductType != ProductType.FeatureOffer || IsOfferActive);
		public string StoreProductId { get; set; } // Apple and google product id
	}
	
	public class PriceWallet
	{
		public float Price { get; set; } = 1;
		public float PriceWithDiscount { get; set; } = 1;
		public int DiscountSize { get; set; }
		public WalletType WalletType { get; set; }

		public bool HasDiscount => PriceWithDiscount < Price || DiscountSize > 0;
		
		public float DiscountRoundedPercent => MathF.Round((1 - (float)PriceWithDiscount / Price) * 100);
	}
}