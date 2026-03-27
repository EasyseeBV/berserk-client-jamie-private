using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Campaign
{
	public class CampaignStageProgressModel
	{
		public int StageIndex { get; set; }
		public string Title { get; set; }
		public string Subtitle { get; set; }
		public CampaignEncounterType EncounterType { get; set; }
		public string BotDeckId { get; set; }
		public Quadrant ArenaTheme { get; set; }
		public bool IsUnlocked { get; set; }
		public bool IsCompleted { get; set; }
		public int Stars { get; set; }
	}
}
