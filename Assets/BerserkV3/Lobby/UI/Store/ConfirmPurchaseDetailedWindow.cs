using System;
using Berserk.Shared.Data.Shop;
using BerserkV3.Lobby.UI.Store.Widgets;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Store
{
	public class ConfirmPurchaseDetailedWindow : ConfirmPurchaseWindow
	{
		[SerializeField] private TMP_Text titleTextLabel;
		[SerializeField] private StoreItemWidget storeItemWidget;

		public override void SetBodyText(string text)
		{
			bodyTextLabel.text = text;
		}

		public void SetTitle(string text)
		{
			titleTextLabel.text = text;
		}

		public void InitItemWidget(ProductModel productModel, Action<ProductModel> clickAction = null)
		{
			storeItemWidget.Init(productModel, clickAction);
		}
	}
}