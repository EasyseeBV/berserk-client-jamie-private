using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Shop;
using BerserkV3.Lobby.UI.Store.Models;

namespace BerserkV3.Lobby.UI.Store.Abstractions
{
	public interface IStoreTabView
	{
		void UpdateContent(StoreTabViewModel tabModel, 
			List<ProductModel> products, 
			ProductModel mainProduct, 
			Action<ProductModel> onBuyButtonPressed);

		void CleanSubscribers();
	}
}