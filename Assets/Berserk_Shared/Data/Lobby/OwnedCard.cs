namespace Berserk.Shared.Data.Lobby
{
	public class OwnedCard
	{
		public string Id { get; set;}
		public string CardId { get; set;}
		public bool IsOwned { get; set; }
		public bool IsSubscription { get; set; }
		public bool IsNft { get; set; }
	}
}