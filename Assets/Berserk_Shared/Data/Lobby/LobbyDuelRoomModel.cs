using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Lobby
{
	public class LobbyDuelRoomModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public bool IsPrivate { get; set; }
		public MatchMode MatchMode { get; set; }
		public List<LobbyPlayerModel> Players { get; set; } = new();

		public PracticeMode Difficulty { get; set; }
		public string HostId { get; set; }
		public string RoomCode { get; set; }
		public string Password { get; set; }
		public bool IsLockedByPassword { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime? StartedDateTime { get; set; }
		public DateTime? ClosedDateTime { get; set; }
	}
}