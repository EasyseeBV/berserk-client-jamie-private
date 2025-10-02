using System.Collections.Generic;
using Newtonsoft.Json;

namespace Berserk.Shared.Data.Lobby
{
	public class LeaderBoardTargetModel
	{
		public List<LeaderBoardScoreByLeagueModelBase> ScoreByLeague { get; set; }
		public string AvatarUrl { get; set; } // TODO is it really needed ? after 28.06.2024 it's empty!
		public long Lava { get; set; }
		[JsonProperty("UserDataUserName")] // TODO remove after vulcan changed
		public string UserName { get; set; }
		public int UserId { get; set; }
		public int UserCommendationLevel { get; set; }
	}
	
	public class LeaderBoardScoreByLeagueModel
	{
		public long Score { get; set; }
		public string LeagueId { get; set; }
		public string LeagueName { get; set; }
		
		public long Wins { get; set; }
		
		public long Loses { get; set; }
		
		public double AverageSessionLengthSec { get; set; }
	}
	
	public class LeaderBoardScoreByLeagueModelBase
	{
		public long ELO { get; set; }
		public string LeagueId { get; set; }
		public string LeagueName { get; set; }
		
		public long Wins { get; set; }
		
		public long Loses { get; set; }
		public long Games { get; set; }
		
		public double AverageSessionLengthSec { get; set; }
	}
	
	public class PublicLeaderBoardScoreByLeagueModel : PublicLeaderBoardScoreByLeagueModelBase
	{
		public string UserName { get; set; }
	}
	
	public class PublicLeaderBoardScoreByLeagueModelBase
	{
		public long ELO { get; set; }
		public string LeagueName { get; set; }
		
		public long Wins { get; set; }
		
		public long Loses { get; set; }
		public long Games { get; set; }
		
		public double AverageSessionLengthSec { get; set; }
	}
}