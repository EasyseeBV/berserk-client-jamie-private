namespace BerserkV3.Common.AnalyticsSystem
{

	public class LoginModel : AnalyticsModel
	{
		private readonly string userName;

		public LoginModel(string userName)
		{
			this.userName = userName;
			Key = EventHelper.LOGIN;
		}

		protected override void FillData()
		{
			base.FillData();
			AddModelParameter(nameof(userName), userName);
		}
	}

}