using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Game
{
	public class RuntimeTimerData : IRuntimeTimerData
	{
		public DateTime EndTime { get; set; }
		public DateTime? Paused { get; set; }
		public int Turn { get; set; }
		public int Round { get; set; }
		public int Duration { get; set; }
		public string OwnerId { get; set; } = string.Empty;
		public TimerState State { get; set; } = TimerState.NotStarted;
		public int TimeHash { get; set; }

		public override int GetHashCode()
		{
			// except TimeHash because it save the cache of hash, it can't be generated on client the same,
			// generate only on server side and used on clients.
			var paused = Paused.HasValue ? Paused.Value.GetHashCode() : 0;
			return HashCode.Combine(EndTime, paused, Turn, Round, OwnerId, (int)State, Duration);
		}
		
	}
}