using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Campaign
{
	public class CampaignStartStageModel
	{
		public Quadrant Quadrant { get; set; }
		public int StageIndex { get; set; }
		public string DeckId { get; set; }
		public bool IsHeroic { get; set; }
	}
}
