using System;

namespace ServerCore.Infrastructure.Models
{
	public class NextRoundModel
	{
		public int RoundNumber { get; set; }
		public DateTime TurnTimerEndsOn { get; set; }
	}
}