using Berserk.Shared.Data.Shop;

namespace Berserk.Shared.Data.Internal
{
	public class ChangeMoneyModel
	{
		public string UserId { get; set; }
		public WalletType Type { get; set; }
		public int Amount { get; set; }
	}
}