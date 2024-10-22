using System.Collections.Generic;
using System.Linq;

namespace Berserk.Shared.Data.Game.DraftMode.Configs
{
	public class DraftModeConfig
	{
		public int MaxRounds { get; set; }
		public int MaxDraftSkips { get; set; }
		public int TokenCost { get; set; }
		// public int MinRoundIndexToSendRequests { get; set; }
		// public double RequestTimeInSeconds { get; set; }
		public List<DraftModeRarityLimit> BetRarityLimits { get; set; } = new();
		public List<DraftModeLimit> DraftLimits { get; set; } = new();
		public List<DraftModeRoundLimit> RoundLimits { get; set; } = new();
		public int MaxCardsLimit => BetRarityLimits.Sum(x => x.MaxCards);
	}
}
