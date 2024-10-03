namespace Berserk.Shared.Data.Identity
{
	public class AuthRegisterModel
	{
		public string Email { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public VersionModel Version { get; set; }
	}
}