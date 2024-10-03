namespace Berserk.Shared.Data.Identity
{
	public class ResetPasswordModel
	{
		public string Email { get; set; }
		public string EmailCode { get; set; }
		public string Password { get; set; }
	}
}