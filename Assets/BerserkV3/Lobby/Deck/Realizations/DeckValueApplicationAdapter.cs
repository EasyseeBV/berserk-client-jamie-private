using Berserk.Shared.Lobby.Abstractions;

namespace BerserkV3.Lobby.Deck
{
	public class DeckValueApplicationAdapter
	{
		public static IDeckValueService Application { get; private set; }

		public DeckValueApplicationAdapter(IDeckValueService deckValueApplication)
		{
			Application = deckValueApplication;
		}
	}
}