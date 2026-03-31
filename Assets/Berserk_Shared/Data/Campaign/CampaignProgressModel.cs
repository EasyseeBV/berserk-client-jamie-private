using System.Collections.Generic;

namespace Berserk.Shared.Data.Campaign
{
	public class CampaignProgressModel
	{
		public bool IsVulcanCityUnlocked { get; set; }
		public int TotalStars { get; set; }
		public int CompletedQuadrants { get; set; }
		public int CompletedCoreQuadrants { get; set; }
		public bool IsAllThreeStarCompleted { get; set; }
		public int ClaimedMilestoneExperience { get; set; }
		public int ClaimedMilestoneCardCount { get; set; }
		public List<string> UnlockedTitles { get; set; } = new();
		public List<string> UnlockedCosmetics { get; set; } = new();
		public List<CampaignTreasureChoiceModel> PendingTreasureChoices { get; set; } = new();
		public List<CampaignTreasureChoiceModel> ActiveTreasures { get; set; } = new();
		public List<CampaignMilestoneRewardModel> Milestones { get; set; } = new();
		public List<CampaignQuadrantProgressModel> Quadrants { get; set; } = new();
	}
}
