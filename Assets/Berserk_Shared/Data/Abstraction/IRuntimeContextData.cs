using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.LogicEvents;
using Newtonsoft.Json;

namespace Berserk.Shared.Data.Abstraction
{
	public interface IRuntimeContextData : IRuntimeDataBase
	{
		[JsonIgnore] bool IsStarted => StartTime.HasValue;
		[JsonIgnore] bool IsEnded => EndTime.HasValue;
		
		string LeagueId { get; }
		MatchMode MatchMode { get; }
		DateTime? EndTime { get; set; }
		DateTime? StartTime { get; set; }
	}
}