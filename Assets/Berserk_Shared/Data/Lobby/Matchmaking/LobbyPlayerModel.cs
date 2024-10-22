namespace Berserk.Shared.Data.Lobby.Matchmaking
{
	public class LobbyPlayerModel
	{
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string AvatarUrl { get; set; }
		public string FrameUrl { get; set; }
		public string DeckId { get; set; }
		public int? MMR { get; set; }
	}
}