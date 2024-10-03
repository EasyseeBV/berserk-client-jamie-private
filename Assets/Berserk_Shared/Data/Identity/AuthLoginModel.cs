namespace Berserk.Shared.Data.Identity
{
	public class AuthLoginModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string DeviceId { get; set; }
		public VersionModel Version { get; set; }
	}
}