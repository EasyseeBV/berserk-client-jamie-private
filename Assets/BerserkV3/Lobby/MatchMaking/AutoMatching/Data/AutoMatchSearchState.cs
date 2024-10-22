namespace BerserkV3.Lobby.MatchMaking.AutoMatching.Data
{
	public enum AutoMatchSearchState
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