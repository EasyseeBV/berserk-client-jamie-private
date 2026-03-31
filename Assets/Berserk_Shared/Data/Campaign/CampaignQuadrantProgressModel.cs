using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Campaign
{
	public class CampaignQuadrantProgressModel
	{
		public Quadrant Quadrant { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public string RewardLabel { get; set; }
		public string ProgressLabel { get; set; }
		public string RewardSummary { get; set; }
		public bool IsFinaleQuadrant { get; set; }
		public bool IsUnlocked { get; set; }
		public bool IsCompleted { get; set; }
		public int CompletedStages { get; set; }
		public int TotalStages { get; set; }
		public bool IsHeroicUnlocked { get; set; }
		public bool IsHeroicCompleted { get; set; }
		public int CompletedHeroicStages { get; set; }
		public string HeroicProgressLabel { get; set; }
		public string HeroicRewardSummary { get; set; }
		public List<CampaignStageProgressModel> Stages { get; set; } = new();
		public List<CampaignStageProgressModel> HeroicStages { get; set; } = new();
	}
}
