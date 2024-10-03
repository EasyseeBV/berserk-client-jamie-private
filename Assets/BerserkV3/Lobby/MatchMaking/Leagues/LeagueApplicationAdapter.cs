namespace BerserkV3.Lobby.MatchMaking.Leagues
{
	public class LeagueApplicationAdapter // TODO remove later it used only while code into views
	{
		public static ILeagueApplication Application { get; private set; }
		
		public LeagueApplicationAdapter(ILeagueApplication application)
		{
			Application = application;
		}
	}
}