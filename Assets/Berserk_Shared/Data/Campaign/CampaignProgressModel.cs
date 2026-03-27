using System.Collections.Generic;

namespace Berserk.Shared.Data.Campaign
{
	public class CampaignProgressModel
	{
		public bool IsVulcanCityUnlocked { get; set; }
		public int TotalStars { get; set; }
		public List<CampaignQuadrantProgressModel> Quadrants { get; set; } = new();
	}
}
