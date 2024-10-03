using System;
using System.Collections.Generic;

namespace ServerCore.Infrastructure.Models
{
	public class SessionContextModel
	{
		public string SessionId { get; set; }
		public HashSet<SessionPlayerModel> Players { get; set; } = new HashSet<SessionPlayerModel>();
		public bool UseLeaderboardStatistics { get; set; }
		public int RoundNumber { get; set; }
		public bool RoundAccepted { get; set; }
		public DateTime? StartDateTime { get; set; }
		public DateTime? EndDateTime { get; set; }
		public DateTime TurnTimerEndsOn { get; set; }
		public DateTime? TurnTimerPausedOn { get; set; }
		public SessionState State { get; set; }
		public long UtcTimeStamp { get; set; }
	}
}