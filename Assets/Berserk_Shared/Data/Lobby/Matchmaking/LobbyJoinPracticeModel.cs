using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Lobby.Matchmaking
{
	public class LobbyJoinPracticeModel
	{
		public MatchMode MatchMode { get; set; }
		public PracticeMode Difficulty { get; set; }
		public string DeckId { get; set; }
	}
}