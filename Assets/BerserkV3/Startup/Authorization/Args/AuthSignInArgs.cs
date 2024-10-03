namespace BerserkV3.Startup.Authorization
{
	public struct AuthSignInArgs : IAuthArg
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public bool AutoSignIn { get; set; }
	}
}