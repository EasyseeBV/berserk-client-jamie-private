using System;
using Berserk.Shared.Data.Shop;
using BerserkV3.Common.PurchasingSystem.Abstractions;
using BerserkV3.Common.UIKit;
using RR.Core.DebugSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BerserkV3.Lobby.UI.Store.Widgets
{
	public class BuyButton : UIViewBase
	{
		private const string LavaSymbol = "<sprite name=\"LavaGemIcon\">";
		private const string CoinSymbol = "<sprite name=\"GoldCoinIcon\">";

		[SerializeField] private Button button;
		[SerializeField] private TMP_Text priceText;

		private IPurchasingApplication purchasingApplication;

		[Inject]
		public void Construct(IPurchasingApplication purchasingApplication)
		{
			this.purchasingApplication = purchasingApplication;
		}

		// TODO if you need an extra event , just add inside the button an event field Action<ProductModel>
		// TODO and union with incoming action and just subscribe button.onClick.AddListener(() => extraAction?.Invoke(productModel));
		public void Init(ProductModel productModel, Action<ProductModel> clickAction)
		{
			// Price without discount currently is not shown in mockups, so showing just one price here
			var price = productModel.DefaultPrice;

			if (price.WalletType == WalletType.Fiat)
			{
				var priceString = "";
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
				try
				{
					// Get price from Unity
					var productData = purchasingApplication.TryGetStoreProductData(productModel.Id);
					priceString = productData.Price.LocalizedPriceString;
				}
				catch (Exception ex)
				{
					RRLogger.Error($"Error in {nameof(Init)} retrieving price for product ID: {productModel.Id} in {nameof(purchasingApplication.TryGetStoreProductData)}. Exception: {ex.Message}");
				}
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
				priceString = $"${price.Price.ToString()}"; // TODO check if we need $ sign
#endif
				priceText.text = priceString;
			}
			else if (price.WalletType == WalletType.AetherCoins) // hardcoded for now, not sure about coin names
			{
				var priceString = $"{CoinSymbol} {price.Price.ToString()}";
				priceText.text = priceString;
			}
			else if (price.WalletType == WalletType.AesSedaiGems) // hardcoded for now, not sure about coin names
			{
				var priceString = $"{LavaSymbol} {price.Price.ToString()}";
				priceText.text = priceString;
			}

			button.onClick.RemoveAllListeners();
			// TODO you should to use closure allocation of your Action because
			// TODO it won't invoke subscribed methods after subscribe. Just use this construction : (arg) => myAction<T>?.Invoke(arg); instead of this : anOtherAction<T> += myAction<T>;
			button.onClick.AddListener(() => clickAction?.Invoke(productModel));
		}

		public void Clear()
		{
			if(button)
				button.onClick.RemoveAllListeners();
		}
	}
}