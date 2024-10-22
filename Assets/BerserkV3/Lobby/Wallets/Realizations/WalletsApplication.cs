using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Shop;
using BerserkV3.Lobby.Store.Network;
using BerserkV3.Lobby.Wallets.Abstractions;
using Berserk.Shared.Data.Shop;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;

namespace BerserkV3.Lobby.Wallets.Realizations
{
	public class WalletsApplication : IWalletsApplication
	{
		public event Action OnWalletsFetched;
		public event Action OnWalletsFetchFailed;
		public event Action<WalletType, int> OnWalletBalanceChanged; 
		
		private readonly IWalletsRepository walletsRepository;

		public WalletsApplication(IWalletsRepository walletsRepository)
		{
			this.walletsRepository = walletsRepository;
		}

		public async UniTask<bool> TryFetchPlayerWalletsAsync()
		{
			try
			{
				walletsRepository.Clear();
				var result = await StoreApi.GetPlayerWallets();

				if (!result.IsSuccess)
				{
					RRLogger.Error($"{nameof(TryFetchPlayerWalletsAsync)} failed;\n" +
					               $"Code: {result.Code}\n" +
					               $"ErrorMessage: {result.ErrorMessage}\n" +
					               $"Message: {result.RawMessage}\n");
					OnWalletsFetchFailed?.Invoke();
					return false;
				}

				if (result.Data == null)
				{
					RRLogger.Error($"{nameof(TryFetchPlayerWalletsAsync)} failed;\n" +
					               $"Wallet Data is null");
					OnWalletsFetchFailed?.Invoke();
					return false;
				}

				var walletsToAdd = new List<PlayerWallet>();
				foreach (var walletModel in (result.Data))
				{
					var walletToAdd = new PlayerWallet(walletModel.Type, walletModel.Amount, walletModel.IsActive);
					walletToAdd.OnAmountChanged += OnWalletBalanceChanged;
					walletsToAdd.Add(walletToAdd);
				}
				walletsRepository.Add(walletsToAdd);
				OnWalletsFetched?.Invoke();
				return true;
			}
			catch (Exception exception)
			{
				RRLogger.Error(exception, $"Error while {nameof(TryFetchPlayerWalletsAsync)} ");
				OnWalletsFetchFailed?.Invoke();
				return false;
			}
		}

		public void UpdateWallet(WalletType type, int amount)
		{
			var wallet = walletsRepository.GetByType(type);
			if (wallet == null)
			{
				RRLogger.Error($"{nameof(TryAddTo)} wallet {type} does not exist in the repository");
				return;
			}

			wallet.SetBalance(amount);
		}

		public bool TryAddTo(WalletType type, int amount)
		{
			var wallet = walletsRepository.GetByType(type);
			if (wallet == null)
			{
				RRLogger.Error($"{nameof(TryAddTo)} wallet {type} does not exist in the repository");
				return false;
			}

			return wallet.TryAdd(amount);
		}

		public bool TrySpendFrom(WalletType type, int amount)
		{
			var wallet = walletsRepository.GetByType(type);
			if (wallet == null)
			{
				RRLogger.Error($"{nameof(TrySpendFrom)} wallet {type} does not exist in the repository");
				return false;
			}

			return wallet.TrySpend(amount);
		}

		public bool HasEnoughAmount(WalletType type, int requiredAmount)
		{
			var wallet = walletsRepository.GetByType(type);
			if (wallet == null)
			{
				RRLogger.Error($"{nameof(HasEnoughAmount)}: Wallet of type '{type}' does not exist in the repository.");
				return false;
			}

			return wallet.Balance >= requiredAmount;
		}
	}
}