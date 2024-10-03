namespace BerserkV3.Startup.Authorization
{
	public struct AuthReferralArgs : IAuthArg
	{
		public AuthButtonArg Skip;
		public AuthButtonArg Submit;
		public string Email;
		public string UserName;

		public static AuthReferralArgs Default(string skippState, string submitState, string email, string userName)
		{
			return new AuthReferralArgs
			{
				Skip = new AuthButtonArg
				{
					State = skippState,
					Text = "Skip"
				},
				Submit = new AuthButtonArg
				{
					State = submitState,
					Text = "Continue"
				},
				Email = email,
				UserName = userName
			};
		}

		public static AuthReferralArgs Default(string nextStateId, string email, string userName)
		{

			return new AuthReferralArgs
			{
				Skip = new AuthButtonArg
				{
					State = nextStateId,
					Text = "Skip"
				},
				Submit = new AuthButtonArg
				{
					State = nextStateId,
					Text = "Continue"
				},
				Email = email,
				UserName = userName
			};
		}
	}
}