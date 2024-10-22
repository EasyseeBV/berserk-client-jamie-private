using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore;

namespace Berserk.Shared.Data.Game
{
	public class RuntimePlayerData : IRuntimePlayerData, IRuntimePlayerInternal
	{
		public string UserId { get; set; }
		public string DeckId { get; set; }
		public string UserName { get; set; }
		public bool IsFirstMover { get; set; }
		public bool IsBot { get; set; }
		public bool IsReady { get; set; }
		public bool IsFinishedMulligan { get; set; }
		public int? TurnsWithoutCards { get; set; }
		public int LastRoundWithActive { get; set; }
		public IntStat Mana { get; set; }
		public IntStat HandCount { get; set; }
		public List<RuntimePlayedCardData> PlayedCards { get; set; } = new();
	}
}