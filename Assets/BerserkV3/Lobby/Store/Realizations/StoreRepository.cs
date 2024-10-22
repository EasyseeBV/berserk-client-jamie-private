using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Shop;
using BerserkV3.Lobby.Store.Abstractions;
using BerserkV3.Lobby.UI.Store.Models;

namespace BerserkV3.Lobby.Store.Realizations
{
	public class StoreRepository : IStoreRepository
	{
		private Dictionary<StoreTabViewModel, List<StoreTabViewModel>> tabsAndSubTabs = new();
		/// <summary>
		/// Products in subtabs in specific tab
		/// </summary>
		private Dictionary<string, Dictionary<string, List<ProductModel>>> allProductsInTab = new();
		/// <summary>
		/// Main products in subtabs in specific tab
		/// </summary>
		private Dictionary<string, Dictionary<string, ProductModel>> mainProductInTab = new();
		private List<ProductTransactionModel> purchaseHistory = new();
		private string featuredTabId;
		
		public string FeaturedTabId => featuredTabId;

		public void AddTabsAndSubTabs(Dictionary<StoreTabViewModel, List<StoreTabViewModel>> tabsAndSubTabs) =>
			this.tabsAndSubTabs = tabsAndSubTabs;

		public void AddProductsToTab(string tabId, Dictionary<string, List<ProductModel>> allProducts)
		{
			allProductsInTab.Add(tabId, allProducts);
		}

		public void AddMainProductsToTab(string tabId, Dictionary<string, ProductModel> mainProducts)
		{
			mainProductInTab.Add(tabId, mainProducts);
		}

		public void AddPurchaseHistory(IEnumerable<ProductTransactionModel> purchaseHistory) =>
			this.purchaseHistory.AddRange(purchaseHistory);
		
		public void SetFeaturedTabId(string tabId) => featuredTabId = tabId;

		public void ClearTabsAndSubTabs() =>
			tabsAndSubTabs.Clear();

		public void ClearAllProductsInTab(string tabId)
		{
			if (allProductsInTab.ContainsKey(tabId))
			{
				allProductsInTab.Remove(tabId);
			}
		}

		public void ClearAllProductsInTabs() =>
			allProductsInTab.Clear();

		public void ClearMainProducts() =>
			mainProductInTab.Clear();

		public void ClearPurchaseHistory() =>
			purchaseHistory.Clear();

		public bool AnyTabsAndSubTabs() =>
			tabsAndSubTabs.Any();

		public bool AnyProductsInTab(string tabId)
		{
			var isTabExist = allProductsInTab.TryGetValue(tabId, out var subTabs);
			if (isTabExist)
			{
				return subTabs.Values.Any(subTab => subTab.Any());
			}

			return false;
		}

		public bool AnyPurchaseHistory() =>
			purchaseHistory.Any();

		public Dictionary<StoreTabViewModel, List<StoreTabViewModel>> GetTabsAndSubTabs(bool excludeHardCurrencyTab = false)
		{
			return tabsAndSubTabs;
		}
		public Dictionary<string, List<ProductModel>> GetProductsInTab(string tabId)
		{
			var isTabExist = allProductsInTab.TryGetValue(tabId, out var subTabs);
			if (isTabExist)
				return subTabs;

			return null;
		}

		public Dictionary<string, ProductModel> GetMainProductsInTab(string tabId)
		{
			var isTabExist = mainProductInTab.TryGetValue(tabId, out var subTabs);
			if (isTabExist)
				return subTabs;

			return null;
		}

		public ProductModel GetProductByIdInTab(string tabId, string storeItemId)
		{
			var isTabExist = allProductsInTab.TryGetValue(tabId, out var subTabs);
			if (isTabExist)
				return subTabs
					.SelectMany(p => p.Value)
					.FirstOrDefault(p => p.Id == storeItemId);

			return null;
		}

		public ProductModel GetMainProductByIdInTab(string tabId, string storeItemId)
		{
			var isTabExist = mainProductInTab.TryGetValue(tabId, out var subTabs);
			if (isTabExist)
				return subTabs.Values.FirstOrDefault(p => p.Id == storeItemId);

			return null;
		}

		public List<ProductTransactionModel> GetPurchaseHistory() =>
			purchaseHistory;

		
		public Dictionary<string, ProductModel> GetAllFeaturedProducts()
		{
			// TODO implement when server data will be completed
			return null;
		}
	}
}