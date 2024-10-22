using System.Collections.Generic;

namespace Berserk.Shared.Data.Lobby.Statistics
{
	public class UserStatisticsData
	{
		public string UserId { get; set; }
		public int Games { get; set; }
		public int WinsAsFirst { get; set; }
		public int Wins { get; set; }
		public int Losses { get; set; }
		public double TimePlayedInSeconds { get; set; }

		public string FavLandId { get; set; }
		public string FavVulcaniteId { get; set; }
		public UserStatisticDeckData FavDeck { get; set; }

		public List<UserStatisticsLeagueData> LeagueStats { get; set; }
		public List<UserStatisticsHeroData> VulcaniteStats { get; set; }
		public List<UserCardStatisticsData> PlayerStatisticsCard { get; set; }
	}
}