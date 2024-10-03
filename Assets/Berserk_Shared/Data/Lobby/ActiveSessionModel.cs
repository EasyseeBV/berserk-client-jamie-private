using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Lobby
{
	public class ActiveSessionModel
	{
		public string Id { get; set; }
		public MatchMode MatchMode { get; set; }
		public List<LobbyPlayerModel> Players { get; set; } = new();
	}
}