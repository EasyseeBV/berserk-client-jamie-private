using System;
using Berserk.Shared.Data.Shop;
using BerserkV3.Lobby.Wallets.Abstractions;
using RR.Core.DebugSystem;

namespace BerserkV3.Lobby.Wallets.Realizations
{
	public class PlayerWallet : IPlayerWallet, IDisposable
	{
		public event Action<WalletType, int> OnAmountChanged;
		public bool IsActive { get; }
		public WalletType Type { get; }
		public int Balance { get; private set; }

		public PlayerWallet(WalletType type, int amount, bool isActive)
		{
			Type = type;
			Balance = amount;
			IsActive = isActive;
		}

		public bool TryAdd(int value)
		{
			if (value <= 0)
			{
				RRLogger.Error($"{nameof(TryAdd)}: The amount to add should be greater than zero.");
				return false;
			}
			
			Set(Balance + value);
			return true;
		}
		
		public bool TrySpend(int value)
		{
			if (value <= 0)
			{
				RRLogger.Error($"{nameof(TrySpend)}: The amount to spend should be greater than zero.");
				return false;
			}
			
			if (!CanSpend(value))
			{
				RRLogger.Error($"{nameof(TrySpend)}: Insufficient funds in wallet of type {Type}.");
				return false;
			}
			
			Set(Balance - value);
			return true;
		}

		public void SetBalance(int value)
		{
			Set(value);
		}

		private void Set(int value)
		{
			Balance = value;
			OnAmountChanged?.Invoke(Type, Balance);
		}
		
		private bool CanSpend(int value)
		{
			return Balance >= value;
		}

		#region IDisposable

		public void Dispose()
		{
			OnAmountChanged = null;
		}

		#endregion
		
	}
}