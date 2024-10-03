namespace BerserkV3.Lobby.MatchMaking.Leagues.Data
{
	public enum MatchSearchState
	{
		Nothing,
		Searching,
		Found,
		Accepted,
		DeclinedOpponent,
		LoadingStartQueue,
		LoadingEndQueue,
		LoadingDeclinedAndEndQueue,
		Disconnected,
		LoadingMatch
	}
}