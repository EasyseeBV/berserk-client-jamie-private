namespace Berserk.Shared.Data.Lobby
{
	public class OwnedVulcanite
	{
		public string Id { get; set;  }
		public string VulcaniteId { get; set; }
		public bool IsOwned { get; set; }
		public bool IsRent { get; set; }
		public bool IsExpireRent { get; set; }
	}
}