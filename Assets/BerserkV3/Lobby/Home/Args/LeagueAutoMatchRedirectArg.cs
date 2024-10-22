using BerserkV3.Lobby.Home.States.GameModes;

namespace BerserkV3.Lobby.Home.Args
{
	public class LeagueAutoMatchRedirectArg : MainMenuRedirectionArg
	{
		public string LeagueId { get; }
		public string DeckId { get; }

		public LeagueAutoMatchRedirectArg(string leagueId, string deckId) : base(nameof(GameModeLeaguesState))
		{
			LeagueId = leagueId;
			DeckId = deckId;
		}
	}
}