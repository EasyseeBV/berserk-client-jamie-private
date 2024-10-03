using System;
using BerserkV3.Common.DataBase;

namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelDeckItemData
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string FactionUrl { get; set; }
		public string CountText { get; set; }
		public string[] LeagueFlagsUrls { get; set; } = Array.Empty<string>();
		public bool IsValid { get; set; }
		public string AvatarUrl { get; set; }
		public string QuadrantUrl { get; set; }
	}
}