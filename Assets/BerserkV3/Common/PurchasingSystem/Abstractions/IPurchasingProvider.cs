using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.PurchasingSystem.Abstractions
{
	public interface IPurchasingProvider
	{
		event Action OnPurchaseProceed;
		event Action OnPurchaseFail;
		event Action OnInitSuccess;
		UniTask InitAsync(CancellationToken token);
		UniTask BuyProductWithIdAsync(string productId);
		void RestorePurchases();
	}
}