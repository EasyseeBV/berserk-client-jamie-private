namespace BerserkV3.Lobby.MatchMaking.Leagues.Data
{
	public readonly struct LeagueAutoMatchRedirectArg
	{
		public string LeagueId { get; }
		public string DeckId { get; }

		public LeagueAutoMatchRedirectArg(string leagueId, string deckId)
		{
			LeagueId = leagueId;
			DeckId = deckId;
		}
	}
}