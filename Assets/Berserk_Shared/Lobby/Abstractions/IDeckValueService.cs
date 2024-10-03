using System.Collections.Generic;
using Berserk.Shared.Data.Lobby;

namespace Berserk.Shared.Lobby.Abstractions
{
	public interface IDeckValueService
	{
		float CalculateDeckValue(IEnumerable<OwnedCard> deckCards);
	}
}