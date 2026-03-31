using Berserk.Shared.Data.Enums;

namespace UI
{
	public readonly struct CampaignRedirectArg
	{
		public readonly Quadrant Quadrant;
		public readonly int StageIndex;
		public readonly string StageTitle;
		public readonly bool IsHeroic;
		public readonly bool DidWin;

		public CampaignRedirectArg(Quadrant quadrant, int stageIndex = 0, string stageTitle = null, bool isHeroic = false, bool didWin = false)
		{
			Quadrant = quadrant;
			StageIndex = stageIndex;
			StageTitle = stageTitle;
			IsHeroic = isHeroic;
			DidWin = didWin;
		}

		public CampaignRedirectArg WithOutcome(bool didWin)
			=> new(Quadrant, StageIndex, StageTitle, IsHeroic, didWin);
	}
}
