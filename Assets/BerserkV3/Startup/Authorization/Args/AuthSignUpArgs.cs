namespace BerserkV3.Startup.Authorization
{
	public struct AuthSignUpArgs : IAuthArg
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public bool AutoSignUp { get; set; }
	}
}