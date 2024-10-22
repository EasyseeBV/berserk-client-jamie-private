using BerserkV3.Lobby.Home.Abstractions;

namespace BerserkV3.Lobby.Home.Args
{
	public class MainMenuRedirectionArg : IMainMenuArg
	{
		public string StateId { get; }

		public MainMenuRedirectionArg(string stateId)
		{
			StateId = stateId;
		}
	}
}