namespace Berserk.Shared.Data.Identity.Social
{
	public class AuthLoginProviderModel
	{
		public string AccessToken { get; set; }
		public int ExpiresIn { get; set; }
		public string RefreshToken { get; set; }
		public string TokenId { get; set; }
		public ExternalProvider ExternalProvider { get; set; }
		public string DeviceId { get; set; }
		public string ReferralCode { get; set; }
		public string Email { get; set; } // only for Apple
		public VersionModel Version { get; set; }
	}
}