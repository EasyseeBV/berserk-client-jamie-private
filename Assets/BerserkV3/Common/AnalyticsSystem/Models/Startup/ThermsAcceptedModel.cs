namespace BerserkV3.Common.AnalyticsSystem
{

	public class ThermsAcceptedModel : AnalyticsModel
	{
		private readonly string userName;

		public ThermsAcceptedModel(string userName)
		{
			this.userName = userName;
			Key = EventHelper.THERMS_ACCEPTED;
		}

		protected override void FillData()
		{
			base.FillData();
			AddModelParameter(nameof(userName), userName);
		}
	}

}