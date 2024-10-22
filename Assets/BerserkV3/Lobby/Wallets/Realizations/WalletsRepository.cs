using System.Collections.Generic;
using Berserk.Shared.Data.Shop;
using BerserkV3.Lobby.Wallets.Abstractions;
using RR.Core.DebugSystem;

namespace BerserkV3.Lobby.Wallets.Realizations
{
	public class WalletsRepository : IWalletsRepository
	{
		private Dictionary<WalletType, IPlayerWallet> playerWallets = new();
		
		public Dictionary<WalletType, IPlayerWallet> GetAll() => 
			playerWallets;

		public IPlayerWallet GetByType(WalletType type)
		{
			return playerWallets.GetValueOrDefault(type);
		}

		public void Add(List<PlayerWallet> walletsToAdd)
		{
			foreach (var playerWallet in walletsToAdd)
			{
				var playerWalletType = playerWallet.Type;
				if (!playerWallets.TryAdd(playerWalletType, playerWallet))
				{
					RRLogger.Error($"{nameof(Add)} {playerWalletType} already exist in dictionary");
					return;
				}
			}
		}

		public void AddWallet(IPlayerWallet wallet)
		{
			var playerWalletType = wallet.Type;
			if (!playerWallets.TryAdd(playerWalletType, wallet))
			{
				RRLogger.Error($"{nameof(AddWallet)} {playerWalletType} already exist in dictionary");
			}
		}

		public void Clear()
		{
			playerWallets.Clear();
		}
	}
}