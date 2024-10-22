using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Game.DraftMode.Configs
{
	public class DraftModeRarityDistribution
	{
		public Rarity Rarity { get; set; }
		public double Chance { get; set; }
		public int MaxCount { get; set; } = 1;
	}
}