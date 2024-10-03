namespace BerserkV3.Common.AnalyticsSystem
{

	public class TutorialBeginModel : AnalyticsModel
	{
		private readonly bool startedForTheFirstTime;
		private readonly string userName;

		public TutorialBeginModel(string userName, bool startedForTheFirstTime = true)
		{
			this.startedForTheFirstTime = startedForTheFirstTime;
			this.userName = userName ?? "Unknown";
			Key = EventHelper.TUTORIAL_BEGIN;
		}

		protected override void FillData()
		{
			base.FillData();
			AddModelParameter(nameof(userName), userName);
			AddModelParameter(nameof(startedForTheFirstTime), startedForTheFirstTime);
		}
	}

}