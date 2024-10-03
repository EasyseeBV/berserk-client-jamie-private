namespace BerserkV3.Startup.Authorization
{
	public struct AuthMessageArgs : IAuthArg
	{
		public string Header;
		public string Message;
		public bool Success;
		public AuthButtonArg ButtonArg;

		public static AuthMessageArgs Retry(
			string state,
			string message,
			string header = null,
			AuthButtonArg? buttonArg = null)
		{
			return new AuthMessageArgs
			{
				Header = header ?? "Heads Up!",
				Message = message,
				Success = false,
				ButtonArg = buttonArg ?? new AuthButtonArg
				{
					State = state,
					Text = "Retry"
				}
			};
		}

		public static AuthMessageArgs Accepted(
			string state,
			string message,
			string header = null,
			AuthButtonArg? buttonArg = null)
		{
			return new AuthMessageArgs
			{
				Header = header ?? "Successfully Accepted",
				Message = message,
				Success = true,
				ButtonArg = buttonArg ?? new AuthButtonArg
				{
					State = state,
					Text = "OK"
				}
			};
		}
	}
}