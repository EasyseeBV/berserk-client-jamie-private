using System.Threading;
using Berserk.Shared.Data.Shop;
using BerserkV3.Lobby.UI.Store.Models;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.PurchasingSystem.Abstractions
{
	public interface IPurchasingApplication
	{
		UniTask InitAsync(CancellationToken token);
		UniTask BuyItemWithStripe(string productId);
		UniTask BuyItemWithUnityPurchasing(string productId);
		void RestorePurchases();

		/// <summary>
		/// Method to get google or store related data for specific product with certain ID
		/// </summary>
		/// <param name="offerId"></param>
		StoreProductViewModel TryGetStoreProductData(string offerId); 
	}
}