namespace Berserk.Shared.Data.Lobby.Statistics
{
	public class UserStatisticsLeagueData
	{
		public string LeagueId { get; set; }
		public int Games { get; set; }
		public int Wins { get; set; }
		public long ELO { get; set; }
	}
}