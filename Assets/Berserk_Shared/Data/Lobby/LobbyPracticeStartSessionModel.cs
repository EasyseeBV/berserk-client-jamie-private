using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Lobby
{
	public class LobbyPracticeStartSessionModel
	{
		public MatchMode MatchMode { get; set; }
		public PracticeMode Difficulty { get; set; }
		public string DeckId { get; set; }
	}
}