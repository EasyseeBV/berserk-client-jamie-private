using System;
using Berserk.Shared.Data.Shop;

using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.Store.Abstractions
{
	public interface IStoreApplication
	{
		event Action OnStoreFetched;
		event Action OnStoreFetchFailed;

		public UniTask FetchStoreAsync();
		public UniTask FetchTabAsync(string tabId);
		public UniTask FetchPurchaseHistoryAsync();
		public UniTask ReFetchProductsAsync(string selectedTab);
		
		public UniTask<ProductTransactionModel> TryBuyItemWithSoftCurrencyAsync(string productId, WalletType walletType);
		public UniTask BuyItemWithHardCurrencyAsync(string productId);
	}
}