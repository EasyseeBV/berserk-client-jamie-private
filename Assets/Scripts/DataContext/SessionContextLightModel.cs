using System;
using System.Collections.Generic;

namespace ServerCore.Infrastructure.Models
{
	public class SessionContextLightModel
	{
		public SessionState State { get; set; }
		public int RoundNumber { get; set; }

		public DateTime? StartDateTime { get; set; }
		public DateTime? EndDateTime { get; set; }
		public string WinnerSessionPlayerId { get; set; }
		public string WinnerUserName { get; set; }
		public string FirstMover { get; set; }
		public string LeagueId { get; set; }
		public string MatchState { get; set; }
		public string SurrenderReason { get; set; }
		public Dictionary<string, string> VulcanitesUsed { get; set; }
	}
}