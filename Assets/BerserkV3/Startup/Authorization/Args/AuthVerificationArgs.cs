using Berserk.Shared.Data.Identity;

namespace BerserkV3.Startup.Authorization
{
	public struct AuthVerificationArgs : IAuthArg
	{
		public string Email { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		public AuthVerificationArgs(AuthRegisterModel model)
		{
			Email = model.Email;
			Password = model.Password;
			UserName = model.UserName;
		}
	}
}