using System;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Abstraction
{
	public interface IRuntimeTimerData : IRuntimeDataBase
	{
		DateTime EndTime { get; set; }
		DateTime? Paused { get; set; }
		int Turn { get; set; }
		int Round { get; set; }
		int Duration { get; set; }
		string OwnerId { get; set; }
		TimerState State { get; set; }
		int TimeHash { get; set; }
	}
}