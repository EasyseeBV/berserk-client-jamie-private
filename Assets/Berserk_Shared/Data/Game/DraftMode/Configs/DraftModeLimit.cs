using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Game.DraftMode.Configs
{
	public class DraftModeLimit
	{
		public DraftStepType DraftType { get; set; }
		public int MaxDraftSteps { get; set; }
		public int AssetsPerStep { get; set; }
		public List<DraftModeRarityDistribution> RarityDistribution { get; set; } = new();
		public List<DraftModeCardTypeDistribution> CardTypeDistribution { get; set; } = new();

		public DraftModeRarityDistribution GetRarityDistribution(Rarity rarity)
		{
			return RarityDistribution.FirstOrDefault(x => x.Rarity == rarity) ?? new();
		}

		public DraftModeCardTypeDistribution GetCardTypeDistribution(ObjectType cardType)
		{
			return CardTypeDistribution.FirstOrDefault(x => x.CardType == cardType) ?? new DraftModeCardTypeDistribution();
		}
	}
}