using System.Collections.Generic;
using Berserk.Shared.Data.Shop;
using BerserkV3.Lobby.Wallets.Realizations;


namespace BerserkV3.Lobby.Wallets.Abstractions
{
	public interface IWalletsRepository
	{
		Dictionary<WalletType, IPlayerWallet> GetAll();
		IPlayerWallet GetByType(WalletType type);
		void Add(List<PlayerWallet> walletsToAdd);
		void Clear();
	}
}