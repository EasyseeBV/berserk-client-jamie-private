using System;
using System.Threading;
using Berserk.Shared.Data.Shop;
using Berserk.Shared.Data.Shop.Enums;
using BerserkV3.Common.EventSource;
using BerserkV3.Common.PurchasingSystem.Abstractions;
using BerserkV3.Common.Utils;
using BerserkV3.Common.WebView.Abstractions;
using BerserkV3.Lobby.Store.Network;
using BerserkV3.Startup.UI;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using Vuplex.WebView;

namespace BerserkV3.Common.PurchasingSystem.Realizations
{
	public class StripePurchasingProvider : IPurchasingProvider
	{
		private IWebViewApplication webViewApplication;
		private readonly IEventsSource eventsSource;
		public StripePurchasingProvider(IWebViewApplication webViewApplication, 
			IEventsSource eventsSource)
		{
			this.webViewApplication = webViewApplication;
			this.eventsSource = eventsSource;
		}

		public event Action OnPurchaseProceed;
		public event Action OnPurchaseFail;
		public event Action OnInitSuccess;

		private bool webViewIsActive;
		private bool stripeSuccessRedirectReceived;
		private IDisposables subscriptions;
		
		public async UniTask InitAsync(CancellationToken token)
		{
			eventsSource.Subscribe<ViewClosedEvent>(HandleWebViewClosed).AddTo(subscriptions);
			eventsSource.Subscribe<UrlChangedEvent>(HandleUrlChanged).AddTo(subscriptions);
		}
		
		public void RestorePurchases()
		{
			//TODO add functionality if required
		}

		public async UniTask BuyProductWithIdAsync(string productId)
		{
			var result = await StoreApi.TryPurchaseIApOffer(productId);

			if (!result.IsSuccess)
			{
				RRLogger.Error($"{nameof(BuyProductWithIdAsync)} failed;\n" +
				               $"Code: {result.Code}\n" +
				               $"ErrorMessage: {result.ErrorMessage}\n" +
				               $"Message: {result.RawMessage}\n");
				//var transactionalModel = new ProductTransactionModel() { Status = TransactionStatus.Failed }; we can use it just in case
				OnPurchaseFail?.Invoke();
				return;
			}

			var stripeUrl = result.Data;
			if (!IsValidUrl(stripeUrl))
			{
				RRLogger.Error($"{nameof(BuyProductWithIdAsync)} failed;\n" +
				               $"Code: {result.Code}\n" +
				               $"ErrorMessage: {result.ErrorMessage}\n" +
				               $"Message: {result.RawMessage}\n");
				OnPurchaseFail?.Invoke();
			}

			webViewIsActive = true;
			stripeSuccessRedirectReceived = false;

			OpenLink(stripeUrl);

			await UniTask.WaitWhile(() => webViewIsActive);

			RRLogger.Log($"Finishing WebView Purchase... stripe redirected to success = {stripeSuccessRedirectReceived}");

			if(stripeSuccessRedirectReceived)
				OnPurchaseProceed?.Invoke();
			else 
				OnPurchaseFail?.Invoke();
		}

		private void HandleWebViewClosed()
		{
			webViewIsActive = false;
		}

		private void HandleUrlChanged(UrlChangedEvent eventData)
		{
			if (!stripeSuccessRedirectReceived)
				stripeSuccessRedirectReceived = eventData.NewUrl.Url.Contains("SuccessPaymentView");
		}

		private bool IsValidUrl(string url)
		{
			if (Uri.TryCreate(url, UriKind.Absolute, out Uri? urlResult))
			{
				return (urlResult.Scheme == Uri.UriSchemeHttp || urlResult.Scheme == Uri.UriSchemeHttps);
			}

			return false;
		}

		private void OpenLink(string url)
		{
			webViewApplication.OpenLink(url);
		}
	}
}