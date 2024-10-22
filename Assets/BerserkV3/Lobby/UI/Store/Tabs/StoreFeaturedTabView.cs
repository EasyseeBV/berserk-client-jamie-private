using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Shop;
using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Store.Abstractions;
using BerserkV3.Lobby.UI.Store.Models;
using BerserkV3.Lobby.UI.Store.Widgets;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Store.Tabs
{
	public class StoreFeaturedTabView : UIViewBase, IStoreTabView
	{
		[SerializeField] private StoreItemWidget mainItemWidget;
		[SerializeField] private List<StoreItemWidget> itemWidgets;

		public void UpdateContent(
			StoreTabViewModel tabModel,
			List<ProductModel> products,
			ProductModel mainProduct,
			Action<ProductModel> onBuyButtonPressed)
		{
			mainItemWidget.Init(mainProduct, onBuyButtonPressed);
			UIHelper.InitWidgets(itemWidgets, products.Count, (w, index) => w.Init(products[index], onBuyButtonPressed));
		}

		public void CleanSubscribers()
		{
			mainItemWidget.BuyButton.Clear();
			foreach (var widget in itemWidgets)
				widget.BuyButton.Clear();
		}
	}
}