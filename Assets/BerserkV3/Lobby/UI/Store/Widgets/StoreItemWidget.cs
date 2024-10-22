using System;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Shop;
using Berserk.Shared.Data.Shop.Enums;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Home.CommonWidgets;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Core.Serialization;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Store.Widgets
{
	[Serializable]
	public class ProductBorderMap : UnitySerializedDictionary<BorderType, FrameWidget>
	{
		public FrameWidget SetOneVisible(BorderType type)
		{
			FrameWidget selected = null;
			foreach (var (key, value) in this)
			{
				if (value)
					value.SetActive(key == type);

				if (key == type)
					selected = value;
			}

			return selected ? selected : this[BorderType.None];
		}

		public void SetAllVisible(bool value)
		{
			Values.ForEach(x=> x.SetActive(value));
		}
	}
	
	public class StoreItemWidget : UIViewBase
	{
		private const string DISCOUNT_LABEL = "<size=200%>{0}%\n<size=100%>Discount";

		[SerializeField] private TMP_Text discountText;
		[SerializeField] private GameObject discountBanner;
		[SerializeField] private BuyButton buyButton;
		[SerializeField] private ProductBorderMap productBorderMap = new();

		private ProductModel model;
		private CancellationTokenSource cancellationTokenSource;
		public BuyButton BuyButton => buyButton;

		public void Init(ProductModel productModel, Action<ProductModel> clickAction)
		{
			model = productModel;
			cancellationTokenSource = new CancellationTokenSource();

			var frameWidget = productBorderMap.SetOneVisible(productModel.BorderType);
			frameWidget.InitAsync(productModel.ArtUrl, cancellationTokenSource.Token).Forget();
			
			if(discountBanner)
				SetupDiscount();

			if(buyButton)
				buyButton.Init(productModel, clickAction);
		}

		private void SetupDiscount()
		{
			var price = model.DefaultPrice;
			var hasDiscount = price.HasDiscount;

			if (hasDiscount)
				discountText.text = string.Format(DISCOUNT_LABEL, price.DiscountSize);

			discountBanner.SetActive(hasDiscount);
		}

		private void OnDestroy()
		{
			if (buyButton)
				buyButton.Clear();
			
			productBorderMap.Values.Where(component => component).ForEach(widget => widget.Clear());
			cancellationTokenSource?.Cancel();
			cancellationTokenSource?.Dispose();
		}
	}
}