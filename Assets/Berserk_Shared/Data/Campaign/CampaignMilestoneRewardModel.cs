namespace Berserk.Shared.Data.Campaign
{
	public class CampaignMilestoneRewardModel
	{
		public int Order { get; set; }
		public string Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsAchieved { get; set; }
		public bool IsClaimable { get; set; }
		public bool IsClaimed { get; set; }
	}
}
