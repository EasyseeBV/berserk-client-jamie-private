using System;

namespace BerserkV3.Common.AnalyticsSystem
{

	public class TutorialCompletedModel : AnalyticsModel
	{
		private readonly string userName;
		private readonly bool completedForTheFirstTime;
		private readonly string elapsedTime;

		public TutorialCompletedModel(string userName, bool completedForTheFirstTime, string elapsedTime)
		{
			this.userName = userName;
			this.completedForTheFirstTime = completedForTheFirstTime;
			this.elapsedTime = elapsedTime;
			Key = EventHelper.TUTORIAL_COMPLETED;
		}

		protected override void FillData()
		{
			base.FillData();
			AddModelParameter(nameof(userName), userName);
			AddModelParameter(nameof(completedForTheFirstTime), completedForTheFirstTime);
			AddModelParameter(nameof(elapsedTime), elapsedTime);
		}
	}

}