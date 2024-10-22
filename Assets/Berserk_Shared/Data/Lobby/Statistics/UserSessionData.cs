using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Lobby.Statistics
{
	public class UserSessionData 
	{
		public string UserDataId { get; set; }
		public string SessionId { get; set; } // TODO rename to roomId
		public string LeagueId { get; set; }
		public string PlayedAvatarId { get; set; }
		public string PlayedVulcaniteId { get; set; }
		public bool HasWon { get; set; }
		public int ELO { get; set; }
		public Faction DeckFaction { get; set; }
		
		public MatchMode MatchMode { get; set; }
		/// <summary>
		/// When the session and players are ready (all players ready to play)
		/// </summary>
		public DateTime? GameReadyDateTime { get; set; }
		/// <summary>
		/// When session completely ended, closed, can't reconnect into.
		/// </summary>
		public DateTime? EndDateTime { get; set; }
		
		public virtual List<string> PlayedDeck { get; set; }
	}
}