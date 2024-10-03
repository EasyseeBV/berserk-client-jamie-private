namespace BerserkV3.Lobby.MatchMaking.Duels
{
	public class DuelsApplicationAdapter
	{
		public static IDuelsApplication Application { get; private set; }
		public DuelsApplicationAdapter(IDuelsApplication application)
		{
			Application = application;
		}
	}
}