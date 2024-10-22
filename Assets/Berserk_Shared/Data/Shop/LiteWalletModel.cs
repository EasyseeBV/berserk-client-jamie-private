using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Shop
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum WalletType
	{
		AetherCoins,
		AesSedaiGems,
		Fiat
	}
	
	public class LiteWalletModel
	{
		public bool IsActive { get; set; }
		public WalletType Type { get; set; }
		public int Amount { get; set; }
	}
}