namespace BerserkV3.Startup.Authorization
{
	public readonly struct AuthRedirectionArg : IAuthArg
	{
		public string StateId { get; }

		public AuthRedirectionArg(string stateId)
		{
			StateId = stateId;
		}
	}
}