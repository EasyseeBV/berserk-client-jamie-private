using Berserk.Shared.Data.Shop;

namespace BerserkV3.Lobby.Wallets.Abstractions
{
	public interface IPlayerWallet
	{
		bool IsActive { get;  }
		WalletType Type { get; } 
		int Balance { get; }
		void SetBalance(int value);
		bool TryAdd(int value);
		bool TrySpend(int value);
	}
}