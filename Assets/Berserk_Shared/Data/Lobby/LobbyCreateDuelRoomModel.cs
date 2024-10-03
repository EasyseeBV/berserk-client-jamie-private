using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Lobby
{
	public class LobbyCreateDuelRoomModel
	{
		public string Name { get; set; }
		public string DeckId { get; set; }
		public MatchMode MatchMode { get; set; }
		public PracticeMode Difficulty { get; set; }
		public string Password { get; set; }
		public bool IsPrivate { get; set; }
	}
}