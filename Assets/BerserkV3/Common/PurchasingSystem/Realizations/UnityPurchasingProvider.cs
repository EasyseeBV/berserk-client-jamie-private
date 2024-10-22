using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Berserk.Shared.Data.Shop;
using Berserk.Shared.Data.Shop.Enums;
using BerserkV3.Common.PurchasingSystem.Abstractions;
using BerserkV3.Common.PurchasingSystem.Models;
using BerserkV3.Lobby.Store.Network;
using BerserkV3.Lobby.UI.Store.Models;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RR.Core.DebugSystem;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;
using ProductType = UnityEngine.Purchasing.ProductType;

namespace BerserkV3.Common.PurchasingSystem.Realizations
{
	public class UnityPurchasingProvider : IPurchasingProvider, IDetailedStoreListener
	{
		//!!MOCK CODE!! TODO Remove after testing
		private const string TEST_ID = "com.test.berserk";
		//!!END OF MOCK CODE!! 
		
		private const string ERROR_PRODUCTS_NOT_FETCHED = "Error_ProductsNotFetched";
		
		public event Action OnPurchaseProceed;
		public event Action OnPurchaseFail;
		public event Action OnInitSuccess;
		public event Action OnInitFail;
		
		private IStoreController storeController;
		private IExtensionProvider storeExtensionProvider;		
		private ConfigurationBuilder _builder;
		private List<ProductModel> definedProducts;
		private static HashSet<StoreProductViewModel> storeValidatedProducts = new ();
		
		public bool IsInitialized => storeController != null
		                              && storeExtensionProvider != null;
		
		public StoreProductViewModel TryGetStoreProductData(string offerId) =>
			storeValidatedProducts.FirstOrDefault(p => p.Id == offerId);
		
		public async UniTask InitAsync(CancellationToken token)
		{
			try
			{
				var result = await StoreApi.GetAllProducts(ContentLanguage.English);
				if (!result.IsSuccess)
				{
					RRLogger.Error($"{nameof(InitAsync)} failed;\n" +
					               $"Code: {result.Code}\n" +
					               $"ErrorMessage: {result.ErrorMessage}\n" +
					               $"Message: {result.RawMessage}\n");
					throw new HttpRequestException(ERROR_PRODUCTS_NOT_FETCHED); 
				}

				var storeProducts = result.Data;
				PopulateDefinedProducts(storeProducts);

				var moduleInstance = StandardPurchasingModule.Instance();
				_builder = ConfigurationBuilder.Instance(moduleInstance);
				
				foreach (var product in definedProducts)
				{
					var storeIDs = new IDs
					{
						{ product.StoreProductId, GooglePlay.Name },
						{ product.StoreProductId, AppleAppStore.Name }
					};
					_builder.AddProduct(product.Id, ProductType.Consumable, storeIDs); // for now just consumables
				}

				UnityPurchasing.Initialize(this, _builder);
			}
			catch (Exception e)
			{
				RRLogger.Error($"{nameof(InitAsync)} failed;\n" +
				               $"ErrorMessage: {e.Message}\n"); // Just try to init in next opening of lobby
			}
		}
		
		public async UniTask BuyProductWithIdAsync(string productId) // TODO implement next flow:
			// press buy => server request =>
			// check if purchase possible on our side =>
			// process purchase thru unity iap =>
			// handle callback => send receipt to server => 
		    // handle new rewards signal
		{
			if (!IsInitialized)
			{
				RRLogger.Log("BuyProductId FAIL: Unity IAP has not been initialized yet");
				return;
			}

			var product = storeController.products.WithID(productId);

			if (product != null && product.availableToPurchase)
			{
				RRLogger.Log($"Purchasing product asynchronously: '{product.definition.id}'");
				var result = await StoreApi.CheckRewardPossible(productId); // Checking on the store if purchase is possible
				if(result.IsSuccess)
				{
					storeController.InitiatePurchase(product);
				}
				else
				{
					OnPurchaseFail?.Invoke();
					RRLogger.Log("BuyProductID: FAIL. Purchase declined by shop server");
				}
			}
			else
			{
				OnPurchaseFail?.Invoke();
				RRLogger.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}

		// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
		// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
		public void RestorePurchases()
		{
			//TODO add functionality if required
		}
		
		#region Unity IAP Listeners
		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) 
		{
			var storeProduct = purchaseEvent.purchasedProduct;
			var storeProductId = storeProduct.definition.id;

			var receipt = GetReceiptData(storeProduct.receipt);
			RRLogger.Log($"ProcessPurchase: {storeProductId} with receipt {receipt}");
		
			if (!IsValidPurchasing(storeProduct.receipt))
			{
				RRLogger.Log($"ProcessPurchase: FAIL. Not validated: '{storeProductId}'");
				// Send to server that reciept not validated
				OnPurchaseFail?.Invoke();
				return PurchaseProcessingResult.Complete;
			}

			var productMatch = definedProducts
				.FirstOrDefault(product => string.Equals(storeProductId, product.Id, StringComparison.Ordinal));

			if (productMatch != null)
			{
#if UNITY_ANDROID
				StoreApi.ValidateGooglePurchase(receipt);
#elif UNITY_IOS
				StoreApi.ValidateApplePurchase(receipt);
#endif
				storeController.ConfirmPendingPurchase(storeProduct);
			}
			else 
			{
				RRLogger.Log($"ProcessPurchase: FAIL. Unrecognized product: {storeProductId}");
				OnPurchaseFail?.Invoke();
				return PurchaseProcessingResult.Complete;
			}

			OnPurchaseProceed?.Invoke();
			return PurchaseProcessingResult.Pending;
		}
		
		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			OnPurchaseFail?.Invoke();
			var storeProductId = product.definition.id;
			
			RRLogger.Log($"ProcessPurchase: FAIL. Id: {storeProductId} reason {failureReason}");
		}
		
		public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
		{
			OnPurchaseFail?.Invoke();
			var storeProductId = product.definition.id;
			
			RRLogger.Log($"ProcessPurchase: FAIL. Id: {storeProductId} reason {failureDescription}");
		}
		
		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			storeController = controller;
			storeExtensionProvider = extensions;
			UpdateProducts();
			RRLogger.Log($"{nameof(UnityPurchasing)} init success.");
			OnInitSuccess?.Invoke();
		}
		
		public void OnInitializeFailed(InitializationFailureReason error)
		{
			RRLogger.Log($"{nameof(UnityPurchasing)} initialization failed. Reason: {error}");
			OnInitFail?.Invoke();
		}

		public void OnInitializeFailed(InitializationFailureReason error, string message)
		{
			RRLogger.Log($"{nameof(UnityPurchasing)} initialization failed. Reason: {error} {message}");
			OnInitFail?.Invoke();
		}
		
		#endregion

		private void PopulateDefinedProducts(List<ProductModel> shopProducts)
		{
			definedProducts = new List<ProductModel>(shopProducts);
		}
		private string GetReceiptData(string receipt)
		{
			var receiptData = string.Empty;
			if (!string.IsNullOrEmpty(receipt))
			{
#if UNITY_EDITOR
				var json = JObject.Parse(receipt);
				receiptData = json.Value<string>("Payload");
#elif UNITY_IOS
				var json = JObject.Parse(receipt);
				receiptData = json.Value<string>("Payload");
#elif UNITY_ANDROID
				var json = JObject.Parse(receipt);
				receiptData = JObject.Parse(json.Value<string>("Payload")).Value<string>("signature");
#endif
			}
			return receiptData;
		}
		
		private bool IsValidPurchasing(string receipt)
		{
			var validPurchase = true; // Presume valid for platforms with no R.V.


#if !UNITY_EDITOR // TODO remove standalone in case of Stripe-only request
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                AppleTangle.Data(), Application.identifier);

            try
            {
                var result = validator.Validate(receipt);

                string str = $"Receipt is valid. Contents:{System.Environment.NewLine}";

                foreach (IPurchaseReceipt productReceipt in result)
                {
                    str += $"Product Id: {productReceipt.productID}{System.Environment.NewLine}";
                    str += $"Purchase Date: {productReceipt.purchaseDate}{System.Environment.NewLine}";
                    str += $"Transaction Id: {productReceipt.transactionID}{System.Environment.NewLine}";
                }

                RRLogger.Log(str);
            }

            catch (IAPSecurityException)
            {
	            RRLogger.Log("Invalid receipt, not unlocking content");
                validPurchase = false;
            }
#endif

			return validPurchase;
		}
		
		private void UpdateProducts()
		{
			if (!IsInitialized)
			{
				Debug.Log("UpdateProducts FAIL: Unity IAP has not been initialized yet");
				return;
			}

			foreach (var product in definedProducts)
			{
				var storeProduct = storeController.products.WithID(product.Id);

				if (storeProduct is null)
				{
					Debug.LogWarning($"Product Fetching FAIL: {product.Id} is not defined on the store");
					continue;
				}

				var newProductModel = FetchProduct(storeProduct, product.Id);
				storeValidatedProducts.Add(newProductModel);
			}
		}

		private StoreProductViewModel FetchProduct(Product storeProduct, string id)
		{
			var price = new ProductPriceModel(
				storeProduct.metadata.isoCurrencyCode,
				storeProduct.metadata.localizedPrice,
				storeProduct.metadata.localizedPriceString
			);

			return new StoreProductViewModel(id, price);
		}
	}
}