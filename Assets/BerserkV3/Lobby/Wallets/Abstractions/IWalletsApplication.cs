using System;
using Berserk.Shared.Data.Shop;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.Wallets.Abstractions
{
	public interface IWalletsApplication
	{
		event Action OnWalletsFetched;
		event Action OnWalletsFetchFailed;
		event Action<WalletType, int> OnWalletBalanceChanged;
		UniTask<bool> TryFetchPlayerWalletsAsync();
		void UpdateWallet(WalletType type, int amount);
		bool TryAddTo(WalletType type, int amount);
		bool TrySpendFrom(WalletType type, int amount);
		bool HasEnoughAmount(WalletType type, int requiredAmount); // temp method, until proper implementation of error messages by server
	}
}