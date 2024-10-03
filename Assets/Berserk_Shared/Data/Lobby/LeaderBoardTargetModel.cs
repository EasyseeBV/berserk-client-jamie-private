namespace Berserk.Shared.Data.Lobby
{
	public class LeaderBoardScoreByLeagueModel
	{
		public long Score { get; set; }
		public string LeagueId { get; set; }
		public string LeagueName { get; set; }
		
		public long Wins { get; set; }
		
		public long Loses { get; set; }
		
		public double AverageSessionLengthSec { get; set; }
	}
}