using System;
using Berserk.Shared.Data.Shop;
using Berserk.Shared.Data.Shop.Enums;
using BerserkV3.Common.UIKit.Abstractions;
using BerserkV3.Common.UIKit.NotifyService.Controllers;
using BerserkV3.Common.UIKit.NotifyService.Models;
using BerserkV3.Lobby.Home.Abstractions;
using BerserkV3.Lobby.Store.Abstractions;
using BerserkV3.Lobby.UI.Store;
using BerserkV3.Lobby.Wallets.Abstractions;
using Cysharp.Threading.Tasks;
using RR.UIService;

namespace BerserkV3.Lobby.Home
{
	public class PurchaseWindowApplication : IPurchaseWindowApplication
	{
		private const string NOT_ENOUGH = "Not enough currency!"; // MOCK TEXT

		private const string LAVA_SYMBOL = "<sprite name=\"LavaGemIcon\">";
		private const string COIN_SYMBOL = "<sprite name=\"GoldCoinIcon\">";

		private readonly IUIService uiService;
		private readonly IStoreApplication storeApplication;
		private readonly IWalletsApplication walletsApplication;
		private readonly INotifyService notifyService;

		private ProductModel productModel;
		private string productPrice = String.Empty;

		public PurchaseWindowApplication(
			IUIService uiService,
			IStoreApplication storeApplication,
			IWalletsApplication walletsApplication,
			INotifyService notifyService)
		{
			this.uiService = uiService;
			this.storeApplication = storeApplication;
			this.walletsApplication = walletsApplication;
			this.notifyService = notifyService;
		}

		public void Init(ProductModel productModel)
		{
			this.productModel = productModel;

			SetupProductPrice();

			var isDetailedViewRequired = productModel.IsDetailedViewRequired;

			if (isDetailedViewRequired)
				OpenDetailedInfoPopUp();
			else
				OpenConfirmationPopUp();
		}

		private void OpenConfirmationPopUp()
		{
			uiService.Begin<ConfirmPurchaseWindow>()
				.WithInit(InitWindow)
				.Show();

			return;

			void InitWindow(ConfirmPurchaseWindow window)
			{
				window.SetPrice(productPrice);
				window.SetBodyText(productModel.Title.ToUpper());
				window.SetCancelAction(CloseConfirmPurchasePopUp);
				window.SetCloseAction(CloseConfirmPurchasePopUp);
				window.SetConfirmAction(ProcessPurchase);
			}
		}

		private void OpenDetailedInfoPopUp()
		{
			uiService.Begin<ConfirmPurchaseDetailedWindow>()
				.WithInit(InitWindow)
				.Show();

			return;

			void InitWindow(ConfirmPurchaseDetailedWindow window)
			{
				window.SetPrice(productPrice);
				window.SetTitle(productModel.Title.ToUpper());
				window.SetBodyText(productModel.Description);
				window.SetCancelAction(CloseDetailedInfoPopUp);
				window.SetCloseAction(CloseDetailedInfoPopUp);
				window.SetConfirmAction(OpenConfirmation);
				window.InitItemWidget(productModel, null); // TODO check
				return;

				void OpenConfirmation()
				{
					CloseDetailedInfoPopUp();
					OpenConfirmationPopUp();
				}
			}
		}

		private void SetupProductPrice()
		{
			var price = productModel.DefaultPrice;

			if (price.WalletType == WalletType.AetherCoins)
			{
				var priceString = $"{COIN_SYMBOL} {price.Price.ToString()}";
				productPrice = priceString;
			}
			else if (price.WalletType == WalletType.AesSedaiGems)
			{
				var priceString = $"{LAVA_SYMBOL} {price.Price.ToString()}";
				productPrice = priceString;
			}
		}

		private void ProcessPurchase()
		{
			ProcessPurchaseAsync().Forget();
		}

		private async UniTask ProcessPurchaseAsync()
		{
			var walletType = productModel.DefaultPrice.WalletType;
			// TODO show wait purchase window when it will be ready
			var purchaseResult = await storeApplication.TryBuyItemWithSoftCurrencyAsync(productModel.Id, walletType);
			// TODO close waiting window
			if (purchaseResult.Status == TransactionStatus.Succeeded)
			{
				//MOCK CODE
				var infoModel = new InfoPopupDataModel()
					.SetTitleText("SUCCESS")
					.SetDescriptionVisibility(false);
				
				notifyService.ShowPopUpAsync<InfoPopUpController>(infoModel);
			}
			else
			{
				//MOCK CODE purchaseResult.RejectedMessage; not ready yet
				var rejectedMessage = "";
				var priceAmount = productModel.DefaultPrice.Price;
				var isEnoughMoney = walletsApplication.HasEnoughAmount(walletType, (int)priceAmount);
				if (!isEnoughMoney)
				{
					rejectedMessage = NOT_ENOUGH;
				}
                //end of MOCK CODE
                var infoModel = new InfoPopupDataModel()
	                .SetBodyText(rejectedMessage);
				notifyService.ShowPopUpAsync<InfoPopUpController>(infoModel);
			}
		}

		private void CloseConfirmPurchasePopUp()
		{
			uiService.Begin<ConfirmPurchaseWindow>().Hide();
		}

		private void CloseDetailedInfoPopUp()
		{
			uiService.Begin<ConfirmPurchaseDetailedWindow>().Hide();
		}
	}
}