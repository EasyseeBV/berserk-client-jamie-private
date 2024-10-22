using System.Collections.Generic;
using Berserk.Shared.Data.Shop;
using BerserkV3.Lobby.UI.Store.Models;
using JetBrains.Annotations;

namespace BerserkV3.Lobby.Store.Abstractions
{
	public interface IStoreRepository
	{
		void AddTabsAndSubTabs(Dictionary<StoreTabViewModel, List<StoreTabViewModel>> tabsAndSubTabs);
		void AddProductsToTab(string tabId, Dictionary<string, List<ProductModel>> allProducts);
		void AddMainProductsToTab(string tabId, Dictionary<string, ProductModel> mainProducts);
		void SetFeaturedTabId(string tabId); // Using only one featured tab based on current mockups
		void AddPurchaseHistory(IEnumerable<ProductTransactionModel> purchaseHistory);
		void ClearTabsAndSubTabs();
		void ClearAllProductsInTab(string tabId);
		void ClearAllProductsInTabs();
		void ClearPurchaseHistory();
		bool AnyTabsAndSubTabs();
		bool AnyProductsInTab(string tabId = "");
		bool AnyPurchaseHistory();
		string FeaturedTabId { get; }

		Dictionary<StoreTabViewModel, List<StoreTabViewModel>> GetTabsAndSubTabs(bool excludeHardCurrencyTab = true);
		Dictionary<string, List<ProductModel>> GetProductsInTab(string tabId);

		[CanBeNull]
		Dictionary<string, ProductModel> GetMainProductsInTab(string tabId);

		ProductModel GetProductByIdInTab(string tabId, string storeItemId);
		ProductModel GetMainProductByIdInTab(string tabId, string storeItemId);
		List<ProductTransactionModel> GetPurchaseHistory();
	}
}