namespace Berserk.Shared.Data.Identity
{
	public class TwoFactorModel
	{
		public bool TwoFactorRequired { get; set; }
		public string TwoFactorSecret { get; set; }
		public UserAuthModel User { get; set; }
	}
}
