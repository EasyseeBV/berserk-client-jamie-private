namespace BerserkV3.Common.AnalyticsSystem
{

	public class TutorialSkippedModel : AnalyticsModel
	{
		private readonly string userName;
		public TutorialSkippedModel(string userName)
		{
			this.userName = userName ?? "Unknown";
			Key = EventHelper.TUTORIAL_SKIPPED;
		}
		
		protected override void FillData()
		{
			base.FillData();
			AddModelParameter(nameof(userName), userName);
		}
	}

}