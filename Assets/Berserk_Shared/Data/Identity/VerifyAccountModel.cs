namespace Berserk.Shared.Data.Identity
{
	public class VerifyAccountModel
	{
		public string Email { get; set; }
		public string UserName { get; set; }
		public string ActivationCode { get; set; }
	}
}