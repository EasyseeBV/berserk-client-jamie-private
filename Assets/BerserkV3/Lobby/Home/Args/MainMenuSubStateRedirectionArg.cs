namespace BerserkV3.Lobby.Home.Args
{
	public class MainMenuSubStateRedirectionArg
	{
		public string StateId { get; }

		public MainMenuSubStateRedirectionArg(string stateId)
		{
			StateId = stateId;
		}

		public override string ToString()
		{
			return $"MainMenuSubStateRedirectionArg (StateId: {StateId})";
		}
	}
}