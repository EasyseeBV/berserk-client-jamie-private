using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Game.DraftMode.Configs
{
	public class DraftModeRoundLimit
	{
		public int RoundNumber { get; set; }
		public int Coins { get; set; }
		public List<DraftModeRoundRarityDistribution> RarityDistribution { get; set; } = new();

		public DraftModeRoundRarityDistribution GetRarityDistribution(Rarity rarity)
		{
			return RarityDistribution.FirstOrDefault(x => x.Rarity == rarity) ?? new();
		}
	}
}