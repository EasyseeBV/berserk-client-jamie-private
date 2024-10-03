namespace Berserk.Shared.Data.Identity.Social
{
	public class SocialAuthRequestModel
	{
		public ExternalProvider ExternalProvider { get; set; }
		public string Code { get; set; }
		public string DeviceId { get; set; }
	}
}