using System;
using System.Collections.Generic;

namespace Berserk.Shared.Data.Lobby
{
	public class LeaderBoardMetaModel
	{
		public string SeasonId { get; set; }
		public List<LeagueShortModel> Leagues { get; set; }
	}
	
	public class LeagueShortModel
	{
		public string LeagueId { get; set; }
		public string LeagueName { get; set; }
	}
}