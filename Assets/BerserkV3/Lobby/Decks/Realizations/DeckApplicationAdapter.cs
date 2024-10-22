namespace BerserkV3.Lobby.Decks
{
	public class DeckApplicationAdapter
	{
		public static  IDeckApplication Application { get; private set; }
		
		public DeckApplicationAdapter(IDeckApplication application)
		{
			Application = application;
		}
	}
}