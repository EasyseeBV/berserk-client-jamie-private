using System;

namespace Berserk.Shared.Data.Lobby.Leagues
{
	public class LeagueAutoMatchStateModel
	{
		public bool IsAutoMatchJoined { get; set; }
		public bool IsMarkedForAutoMatch { get; set; }
		public bool IsMatchFound { get; set; }
		public bool? IsMatchAccepted { get; set; }
		public DateTime? AcceptionTimeOut { get; set; }
	}
}