namespace Berserk.Shared.SignalR.Lobby
{
	public class RankedDecaySignalDto
	{
		public int OldRanked { get; set; }
		public int NewRanked { get; set; }
		public int DecaySize { get; set; }
		public string LeagueName { get; set; }
		public string SeasonId { get; set; }
	}
}