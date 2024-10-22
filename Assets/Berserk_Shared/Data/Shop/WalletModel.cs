using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Berserk.Shared.Data.Shop
{
	public class WalletModelBase
	{
		public WalletModel Wallet { get; set; }
	}

	public class WalletModel
	{
		public bool IsActive { get; set; }
		public string Address { get; set; }
		public TokenModel Tokens { get; set; }
	}

	public class TokenModel
	{
		[JsonProperty("$AEG")]
		public AEGModel AEG { get; set; }
		public List<JObject> Cards { get; set; } = new List<JObject>();
	}
	public class AEGModel
	{
		public decimal Amount { get; set; }
		public decimal LockedAmount { get; set; }
		public decimal SpentAmount { get; set; }
	}
}

