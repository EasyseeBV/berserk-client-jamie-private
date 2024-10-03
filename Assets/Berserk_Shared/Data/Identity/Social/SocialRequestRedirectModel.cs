namespace Berserk.Shared.Data.Identity.Social
{
	public class SocialRequestRedirectModel
	{
		public ExternalProvider? ExternalProvider { get; set; }
		public string DeviceId { get; set; }
	}
}