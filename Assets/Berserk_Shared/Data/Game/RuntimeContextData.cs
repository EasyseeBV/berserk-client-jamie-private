using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Game
{
	[Serializable]
	public class RuntimeContextData : IRuntimeContextData
	{
		public string LeagueId { get; set; }
		public MatchMode MatchMode { get; set; }
		public DateTime? EndTime { get; set; }
		public DateTime? StartTime { get; set; }
		
		public void Started(DateTime value)
		{
			StartTime = value;
		}

		public void Ended(DateTime value)
		{
			EndTime = value;
		}
	}
}