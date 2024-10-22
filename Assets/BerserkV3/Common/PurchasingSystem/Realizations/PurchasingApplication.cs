using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Shop;
using BerserkV3.Common.PurchasingSystem.Abstractions;
using BerserkV3.Lobby.UI.Store.Models;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.PurchasingSystem.Realizations
{
	public class PurchasingApplication : IPurchasingApplication
	{
		public event Action OnPurchaseProceed;
		public event Action OnPurchaseFail;
		
		private readonly IEnumerable<IPurchasingProvider> purchasingProviders;

		private UnityPurchasingProvider unityPurchasingProvider;
		private StripePurchasingProvider stripePurchasingProvider;

		public PurchasingApplication(IEnumerable<IPurchasingProvider> purchasingProviders)
		{
			this.purchasingProviders = purchasingProviders;
		}

		public async UniTask InitAsync(CancellationToken token)
		{
			await UniTask.WhenAll(purchasingProviders.Select(x => x.InitAsync(token))).AttachExternalCancellation(token);
			SetUnityPurchasingProvider();
			SetStripePurchasingProvider();
			SubscribeProviders();
		}

		public async UniTask BuyItemWithStripe(string productId)
		{
			await stripePurchasingProvider.BuyProductWithIdAsync(productId);
		}

		public async UniTask BuyItemWithUnityPurchasing(string productId)
		{
			await unityPurchasingProvider.BuyProductWithIdAsync(productId);
		}

		public void RestorePurchases() =>
			unityPurchasingProvider.RestorePurchases();

		public StoreProductViewModel TryGetStoreProductData(string offerId)
		{
			return unityPurchasingProvider.TryGetStoreProductData(offerId);
		}

		private void SetUnityPurchasingProvider()
		{
			unityPurchasingProvider = purchasingProviders.OfType<UnityPurchasingProvider>().FirstOrDefault();
		}
		
		private void SetStripePurchasingProvider()
		{
			stripePurchasingProvider = purchasingProviders.OfType<StripePurchasingProvider>().FirstOrDefault();
		}

		private void SubscribeProviders()
		{
			foreach (var provider in purchasingProviders)
			{
				provider.OnPurchaseProceed += OnPurchaseProceed;
				provider.OnPurchaseFail += OnPurchaseFail;
			}
		}
	}
}