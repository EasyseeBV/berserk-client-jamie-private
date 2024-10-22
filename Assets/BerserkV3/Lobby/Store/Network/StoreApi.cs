using System.Collections.Generic;
using Berserk.Shared.Data.Shop;
using Berserk.Shared.Data.Shop.Enums;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using Berserk.Shared.Data.Shop;
using Cysharp.Threading.Tasks;
using RR.Network.Rest;

namespace BerserkV3.Lobby.Store.Network
{
	public class StoreApi : API<StoreApi>
	{
		public override string BaseUrl => URLs.ShopUrl;
		public override string AuthToken => User.AccessToken;
		
		public static async UniTask<APIResponse<List<LiteWalletModel>>> GetPlayerWallets() =>
			await GetAsync<List<LiteWalletModel>>($"Wallet/GetLiteWallets").AsUniTask();

		public static async UniTask<APIResponse<ShopTabFullModel>> GetStoreTab(string shopTabId, ContentLanguage language) =>
			await GetAsync<ShopTabFullModel>($"Product/GetShopTabFull?{nameof(shopTabId)}={shopTabId}&{nameof(language)}={language}").AsUniTask();

		public static async UniTask<APIResponse<List<ShopTabFullModel>>> GetAllStore(ContentLanguage language) =>
			await GetAsync<List<ShopTabFullModel>>($"Product/GetAllShopTabsFull?{nameof(language)}={language}").AsUniTask();
		
		public static async UniTask<APIResponse<List<ShopTabLightModel>>> GetAllStoreLight(ContentLanguage language) =>
			await GetAsync<List<ShopTabLightModel>>($"Product/GetAllShopTabsLight?{nameof(language)}={language}").AsUniTask();
		
		public static async UniTask<APIResponse<List<ProductModel>>> GetAllProducts(ContentLanguage language) =>
			await GetAsync<List<ProductModel>>($"Product/GetAllProducts?{nameof(language)}={language}").AsUniTask();  //TODO Add this method to the store

		public static async UniTask<APIResponse<ProductTransactionModel>> TryPurchaseProduct(string productId, WalletType walletType) =>
			await PostAsync<ProductTransactionModel>($"Product/PurchaseProduct?{nameof(productId)}={productId}&{nameof(walletType)}={walletType}", string.Empty).AsUniTask();

		public static async UniTask<APIResponse<string>> TryPurchaseIApOffer(string iApOfferId) =>
			await GetAsync<string>($"IApOffer/PurchaseIApOffer?{nameof(iApOfferId)}={iApOfferId}").AsUniTask();

		public static async UniTask<APIResponse<List<ProductTransactionModel>>> GetPurchaseHistory() =>
			await GetAsync<List<ProductTransactionModel>>("Product/GetPurchaseHistory").AsUniTask();
		
		// Pre purchase checkup
		public static async UniTask<APIResponse<string>> CheckRewardPossible(string offerId) =>
			await GetAsync<string>($"Product/CheckRewardPossible?{nameof(offerId)}={offerId}").AsUniTask();
		
		// After purchase validation posts
		public static async UniTask ValidateGooglePurchase(string receipt) =>
			await PostAsync($"ExternalPurchase/ValidateGooglePurchase", receipt).AsUniTask();
		
		public static async UniTask ValidateApplePurchase(string receipt) =>
			await PostAsync($"ExternalPurchase/ValidateApplePurchase", receipt).AsUniTask();
	}
}