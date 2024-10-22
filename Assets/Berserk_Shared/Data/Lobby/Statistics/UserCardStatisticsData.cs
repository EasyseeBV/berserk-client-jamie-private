namespace Berserk.Shared.Data.Lobby.Statistics
{
	public class UserCardStatisticsData
	{
		public string CardId { get; set; }
		public float Winrate { get; set; }
		public int CountGames { get; set; }
		public int ELO { get; set; }
		public int Power { get; set; }
		public int Level { get; set; }
		public int Own { get; set; }
	}
}