using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Berserk.Shared.Data.Shop;
using Berserk.Shared.Data.Shop.Enums;
using BerserkV3.Common.PurchasingSystem.Abstractions;
using BerserkV3.Lobby.Store.Abstractions;
using BerserkV3.Lobby.Store.Network;
using BerserkV3.Lobby.UI.Store.Models;
using BerserkV3.Lobby.Wallets.Abstractions;

using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;

namespace BerserkV3.Lobby.Store.Realizations
{
	public class StoreApplication : IStoreApplication
	{
		private const string ERROR_PRODUCTS_NOT_FETCHED = "Error_ProductsNotFetched";
		
		private readonly IStoreRepository storeRepository;
		private readonly IWalletsApplication walletsApplication;
		private readonly IPurchasingApplication purchasingApplication;

		public event Action OnStoreFetched;
		public event Action OnStoreFetchFailed;
		
		public StoreApplication(IStoreRepository storeRepository, 
			IWalletsApplication walletsApplication, 
			IPurchasingApplication purchasingApplication)
		{
			this.storeRepository = storeRepository;
			this.walletsApplication = walletsApplication;
			this.purchasingApplication = purchasingApplication;
		}
		
		public async UniTask FetchStoreAsync()
		{
			try
			{
				var result = await StoreApi.GetAllStoreLight(ContentLanguage.English);

				if (!result.IsSuccess)
				{
					RRLogger.Error($"{nameof(FetchStoreAsync)} failed;\n" +
					               $"Code: {result.Code}\n" +
					               $"ErrorMessage: {result.ErrorMessage}\n" +
					               $"Message: {result.RawMessage}\n");
					throw new HttpRequestException(ERROR_PRODUCTS_NOT_FETCHED);
				}

				var tabsAndSubTabsWithProducts = result.Data;

				if (tabsAndSubTabsWithProducts == null)
				{
					RRLogger.Error($"{nameof(FetchStoreAsync)} {nameof(tabsAndSubTabsWithProducts)} is null");
					throw new HttpRequestException(ERROR_PRODUCTS_NOT_FETCHED); 
				}

				PopulateTabsAndSubTabs(tabsAndSubTabsWithProducts);

				await FetchPurchaseHistoryAsync();

				OnStoreFetched?.Invoke();
			}
			catch (Exception exception)
			{
				OnStoreFetchFailed?.Invoke();
				RRLogger.Error(exception, $"Error while {nameof(FetchStoreAsync)} ");
			}
		}

		public async UniTask FetchTabAsync(string tabId)
		{
			var isAnyProducts = storeRepository.AnyProductsInTab(tabId);
			var resultForSelectedTab = await StoreApi.GetStoreTab(tabId, ContentLanguage.English);

			if (!resultForSelectedTab.IsSuccess)
			{
				RRLogger.Error($"{nameof(FetchTabAsync)} failed;\n" +
				               $"Code: {resultForSelectedTab.Code}\n" +
				               $"ErrorMessage: {resultForSelectedTab.ErrorMessage}\n" +
				               $"Message: {resultForSelectedTab.RawMessage}\n");
				throw new HttpRequestException(ERROR_PRODUCTS_NOT_FETCHED);
			}

			var subTabsWithProducts = resultForSelectedTab.Data;

			if (subTabsWithProducts == null)
			{
				RRLogger.Error($"{nameof(FetchTabAsync)} {nameof(subTabsWithProducts)} is null");
				throw new HttpRequestException(ERROR_PRODUCTS_NOT_FETCHED); 
			}

			if (!isAnyProducts)
				PopulateProductsTab(tabId, subTabsWithProducts);
		}

		public async UniTask FetchPurchaseHistoryAsync()
		{
			try
			{
				if (storeRepository.AnyPurchaseHistory())
					return;

				var result = await StoreApi.GetPurchaseHistory();

				if (!result.IsSuccess)
				{
					RRLogger.Error($"{nameof(FetchPurchaseHistoryAsync)} failed;\n" +
					               $"Code: {result.Code}\n" +
					               $"ErrorMessage: {result.ErrorMessage}\n" +
					               $"Message: {result.RawMessage}\n");
					throw new HttpRequestException(ERROR_PRODUCTS_NOT_FETCHED);
				}

				var purchaseHistories = result.Data;

				if (purchaseHistories == null)
				{
					RRLogger.Error($"{nameof(FetchPurchaseHistoryAsync)} {nameof(purchaseHistories)} is null");
					throw new HttpRequestException(ERROR_PRODUCTS_NOT_FETCHED); 
				}
				
				RRLogger.Log($"{nameof(FetchPurchaseHistoryAsync)} received {purchaseHistories.Count} PurchaseHistory");

				PopulatePurchaseHistory(purchaseHistories);
			}
			catch (Exception exception)
			{
				RRLogger.Error(exception, $"Error while {nameof(FetchPurchaseHistoryAsync)}");
			}
		}

		public async UniTask ReFetchProductsAsync(string selectedTab)
		{
			storeRepository.ClearAllProductsInTabs();
			storeRepository.ClearTabsAndSubTabs();
			storeRepository.ClearPurchaseHistory();
			await walletsApplication.TryFetchPlayerWalletsAsync();
			await FetchStoreAsync();
			await FetchTabAsync(selectedTab);
		}

		public async UniTask<ProductTransactionModel> TryBuyItemWithSoftCurrencyAsync(string productId, WalletType walletType)
		{
			var result = await StoreApi.TryPurchaseProduct(productId, walletType);
			
			if (!result.IsSuccess)
			{
				RRLogger.Error($"{nameof(TryBuyItemWithSoftCurrencyAsync)} failed;\n" +
				               $"Code: {result.Code}\n" +
				               $"ErrorMessage: {result.ErrorMessage}\n" +
				               $"Message: {result.RawMessage}\n");
				return new ProductTransactionModel() { Status = TransactionStatus.Failed };
			}

			await RefetchPurchaseHistoryAsync();

			var purchaseResult = result.Data;
			walletsApplication.UpdateWallet(walletType, (int)purchaseResult.RemainingMoney);
			
			return purchaseResult;
		}

		public async UniTask BuyItemWithHardCurrencyAsync(string productId)
		{
			// Temporary solution just to check payments? until selection popup info will not arrive from Vulcan. Task to add popup #86c0d6tww
#if UNITY_ANDROID || UNITY_IOS
			await purchasingApplication.BuyItemWithUnityPurchasing(productId);
#elif UNITY_STANDALONE_WIN
			await purchasingApplication.BuyItemWithStripe(productId);
#endif
		}

		private void PopulateTabsAndSubTabs(List<ShopTabLightModel> tabsAndSubTabs)
		{
			Dictionary<StoreTabViewModel, List<StoreTabViewModel>> tabsAndSubTabsDictionary = new();

			foreach (var tab in tabsAndSubTabs)
			{
				if (tab.ParentShopTabId != null)
					continue;

				var subTabs = tab.SubTabs
					.Select(subTab => new StoreTabViewModel(subTab.Id, subTab.Title, subTab.Description, true, subTab.IsComingSoon, subTab.IsFeatured))
					.ToList();
				
				if (tab.IsFeatured)
					storeRepository.SetFeaturedTabId(tab.Id);

				tabsAndSubTabsDictionary.Add(new StoreTabViewModel(tab.Id, tab.Title, tab.Description, tab.SubTabs.Count == 0, tab.IsComingSoon, tab.IsFeatured), subTabs);
			}

			storeRepository.ClearTabsAndSubTabs();
			storeRepository.AddTabsAndSubTabs(tabsAndSubTabsDictionary);
		}

		private void PopulateProductsTab(string tabId, ShopTabFullModel tab)
		{
			Dictionary<string, List<ProductModel>> productsInTAb = new();
			Dictionary<string, ProductModel> tabsAndMainProductsDictionary = new();

			if (tab.ChildShopTabs.Count == 0 && tab.ParentShopTabId == null)
			{
				if (tab.Products.Any())
					productsInTAb.Add(tab.Id, tab.Products.OrderBy(p => p.OrderAsc).ToList());

				if (tab.MainProduct != null)
					tabsAndMainProductsDictionary.Add(tab.Id, tab.MainProduct);
			}

			foreach (var subTab in tab.ChildShopTabs)
			{
				if (subTab.Products.Any())
					productsInTAb.Add(subTab.Id, subTab.Products.OrderBy(p => p.OrderAsc).ToList());

				if (subTab.MainProduct != null)
					tabsAndMainProductsDictionary.Add(subTab.Id, subTab.MainProduct);
			}

			storeRepository.ClearAllProductsInTab(tabId);
			storeRepository.AddProductsToTab(tabId, productsInTAb);
			storeRepository.AddMainProductsToTab(tabId, tabsAndMainProductsDictionary);
		}

		private void PopulatePurchaseHistory(IEnumerable<ProductTransactionModel> purchaseHistory)
		{
			storeRepository.ClearPurchaseHistory();
			storeRepository.AddPurchaseHistory(purchaseHistory);
		}
		
		private async UniTask RefetchPurchaseHistoryAsync()
		{
			storeRepository.ClearPurchaseHistory();
			await FetchPurchaseHistoryAsync();
		}
	}
}