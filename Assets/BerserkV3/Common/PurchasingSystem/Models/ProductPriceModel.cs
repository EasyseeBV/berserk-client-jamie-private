namespace BerserkV3.Common.PurchasingSystem.Models
{
	/// <summary>
	/// Model of product price specifically made for google and apple
	/// </summary>
	public class ProductPriceModel
	{
		public string CurrencyCode { get; } //ISO Code //USD,TRY
		public decimal LocalizedPrice { get; } ////155.49
		public string LocalizedPriceString { get; } //155.49 $ or €

		public ProductPriceModel(string currencyCode, decimal localPrice, string strLocalPrice)
		{
			CurrencyCode = currencyCode;
			LocalizedPrice = localPrice;
			LocalizedPriceString = strLocalPrice;
		}
	}
}