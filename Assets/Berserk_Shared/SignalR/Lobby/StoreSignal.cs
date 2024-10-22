using System.Collections.Generic;
using Berserk.Shared.Data.Shop;
using Berserk.Shared.SignalR.Abstractions;
using Berserk.Shared.SignalR.Enums;

namespace Berserk.Shared.SignalR.Lobby
{
	public class StoreSignal : ISocketSignal
	{
		public SignalType Type => SignalType.Store;
		
		public List<ItemModel> PurchasedItems { get; }
		public StoreSignal(List<ItemModel> purchasedItems)
		{
			PurchasedItems = new List<ItemModel>(purchasedItems);
		}
	}
}