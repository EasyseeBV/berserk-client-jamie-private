using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Shop.Enums;

namespace Berserk.Shared.Data.Shop
{
	public class ProductTransactionModel
	{
		public string TransactionId { get; set; }
		public string ProductId { get; set; }
		public string ProductName { get; set; }
		public string ProductArtUrl { get; set; }
		public List<ItemModel> CurrentReward { get; set; } = new();
		public float Price { get; set; }
		public float RemainingMoney { get; set; }
		public WalletType WalletType { get; set; }
		public TransactionStatus Status { get; set; }
		public string RejectedMessage { get; set; }
		public DateTime CreatedOn { get; set; }
	}
}